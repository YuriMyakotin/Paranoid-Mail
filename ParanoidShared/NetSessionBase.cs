using System;
using Dapper;
using System.Net.Sockets;
using System.IO;
using HashLib.Crypto.SHA3;
using ProtoBuf;
using System.Security.Cryptography;
using SevenZip;
using static Paranoid.LongTime;

namespace Paranoid
{
	//-----------------------------------------------------------------------------------------------------
	public class NetSession:IDisposable
	{
		protected ushort UseVersionHi; // for future
		protected ushort UseVersionLo;

		protected ulong SelectedSendEncryptionType;
		protected ulong SelectedRecvEncryptionType;

		protected const ushort SupportedVersionHi = 1; //current protocol version
		protected const ushort SupportedVersionLo = 0;

		protected const ulong SupportedEncryptionFlags = (ulong) CryptoProtocols.ChaCha20 | (ulong) CryptoProtocols.ThreeFish256 | (ulong) CryptoProtocols.ThreeFish512;


		protected readonly NetworkEncryption NE;

		public byte[] SendBuff;
		public byte[] RecvBuff;

		public TcpClient TcpClnt;
		public Socket Sock;

		protected long MyServerID;
		protected long MyUserID;

		protected long RemoteServerID;
		protected long RemoteUserID;

		public uint RecvCmdSize;
		public byte RecvCmdCode;

		public volatile NetSessionResult EndResult;

		public readonly Random Rnd;
		private readonly int PercentsOfNoise;

		public string PortPassword = null;

		//-----------------------------------------------------------------------------------------------------
		public NetSession()
		{

			using (RNGCryptoServiceProvider CRNG = new RNGCryptoServiceProvider())
			{
				byte[] buf=new byte[4];
				CRNG.GetBytes(buf);
				Rnd=new Random(BitConverter.ToInt32(buf,0));
			}
			NE = new NetworkEncryption();
			PercentsOfNoise = Rnd.Next(10, 50);
			if (PercentsOfNoise < 25) PercentsOfNoise = 5;
			if (PercentsOfNoise > 35) PercentsOfNoise = 45;

		}

		//-----------------------------------------------------------------------------------------------------
		protected bool RecvData(int size)
		{

			int cbAlreadyReceived = 0;
			RecvBuff = new byte[size];

			while (cbAlreadyReceived < size)
			{
				int cbReceived;
				try
				{
					cbReceived = Sock.Receive(RecvBuff, cbAlreadyReceived, size - cbAlreadyReceived, SocketFlags.None);
				}
				catch
				{
					return false;
				}
				if (cbReceived == 0) return false;
				cbAlreadyReceived += cbReceived;
			}

			return true;
		}

		//-----------------------------------------------------------------------------------------------------
		protected bool SendData() => SendData(SendBuff);

		private bool SendData(byte[] Data)
		{
			try
			{
				if (Sock.Send(Data) == 0) return false;
			}
			catch
			{
				return false;
			}
			return true;
		}


		//-----------------------------------------------------------------------------------------------------
		public static byte[] MakeCmd(CmdCode CmdType)
		{

			uint MsgHeader = (byte)CmdType;
			return BitConverter.GetBytes(MsgHeader);

		}

		//-----------------------------------------------------------------------------------------------------
		public static byte[] MakeCmd(CmdCode CmdType, byte[] Data, int DataLen, int DataOffset)
		{
			byte[] NewData = new byte[DataLen + sizeof(uint)];
			Buffer.BlockCopy(Data, DataOffset, NewData, sizeof(uint), DataLen);
			uint MsgHeader = (uint)((DataLen) << 8) + (byte)CmdType;
			byte[] tmp = BitConverter.GetBytes(MsgHeader);
			Buffer.BlockCopy(tmp, 0, NewData, 0, sizeof(uint));
			return NewData;
		}

		public static byte[] MakeCmd(CmdCode CmdType, long Data)
		{
			byte[] NewData=new byte[12];
			uint MsgHeader = 2048U + (byte)CmdType;
			byte[] tmp = BitConverter.GetBytes(MsgHeader);
			Buffer.BlockCopy(tmp, 0, NewData, 0,4);
			tmp = BitConverter.GetBytes(Data);
			Buffer.BlockCopy(tmp,0,NewData,4,8);
			return NewData;
		}

		public static byte[] MakeCmd(CmdCode CmdType, int Data)
		{
			byte[] NewData = new byte[8];
			uint MsgHeader = 1024U + (byte)CmdType;
			byte[] tmp = BitConverter.GetBytes(MsgHeader);
			Buffer.BlockCopy(tmp, 0, NewData, 0, 4);
			tmp = BitConverter.GetBytes(Data);
			Buffer.BlockCopy(tmp, 0, NewData, 4, 4);
			return NewData;
		}


		//-----------------------------------------------------------------------------------------------------
		public static byte[] MakeCmd<T>(CmdCode CmdType, T Obj)
		{
			byte[] buf1;
			using (var MS = new MemoryStream())
			{
				Serializer.Serialize<T>(MS, Obj);
				buf1 = MS.ToArray();
			}

			byte[] buf = new byte[buf1.Length + 4];
			uint MsgHeader = (uint)((buf1.Length) << 8) + (byte)CmdType;
			byte[] tmp = BitConverter.GetBytes(MsgHeader);
			Buffer.BlockCopy(tmp, 0, buf, 0, sizeof(uint));
			Buffer.BlockCopy(buf1,0,buf,4,buf1.Length);

			return buf;
		}

		//-----------------------------------------------------------------------------------------------------
		public NetSessionResult Connect(Server RemoteServer)
		{

			RemoteServerID = RemoteServer.ServerID;

			try
			{
				TcpClnt = new TcpClient(RemoteServer.IP, RemoteServer.Port);
			}
			catch
			{
				return NetSessionResult.CantConnect;
			}

			Sock = TcpClnt.Client;
			Sock.ReceiveTimeout = NetworkVariables.SocketTimeout;
			Sock.SendTimeout = NetworkVariables.SocketTimeout;
			Sock.Blocking = true;
			Sock.ReceiveBufferSize = NetworkVariables.SocketBufferSize;
			Sock.SendBufferSize = NetworkVariables.SocketBufferSize;

			//stage 1 - check server and synchronize versions
			MakeHandshakeBytes(SupportedVersionHi,SupportedVersionLo,SupportedEncryptionFlags,0,RemoteServerID);
			if (!SendData()) return NetSessionResult.NetError;
			NE.AddHashData(SendBuff);

			if (!RecvData(128)) return NetSessionResult.NetError;
			NE.AddHashData(RecvBuff);
			if (!CheckHandshakeData(RemoteServerID))
			{
				return NetSessionResult.InvalidData;
			}


			if ((UseVersionHi * 65536 + UseVersionLo) > (SupportedVersionHi * 65536 + SupportedVersionLo)) return NetSessionResult.InvalidData;
			NE.Init(SelectedSendEncryptionType,SelectedRecvEncryptionType);

			//stage 2 - send pub keys and random
			SendBuff = NE.MakePublicKeysBlock();
			if (!SendData()) return NetSessionResult.NetError;
			NE.AddHashData(SendBuff);

			SendBuff = new byte[64];
			ParanoidRNG.GetBytes(SendBuff);
			if (!SendData()) return NetSessionResult.NetError;
			NE.AddHashData(SendBuff);

			//stage 3 - checking server signature and making shared keys
			RecvData(NetworkEncryption.KeyBlockSize);

			byte[] SrvKeys = RecvBuff;
			NE.AddHashData(SrvKeys);

			RecvData(64);
			byte[] Sig = RecvBuff;

			if (!NE.VerifySignature(Sig, RemoteServer.CurrentPublicKey))
				return NetSessionResult.InvalidCredentials;

			NE.SetSharedKey(SrvKeys,true);


			return NetSessionResult.Ok;
		}

		public NetSessionResult Connect(long ServerID)
		{
			Server RemoteSrv;
			using (DB DBC=new DB())
			{

				RemoteSrv =
					DBC.Conn.QueryFirstOrDefault<Server>("Select * from Servers where ServerID=@SrvID", new { SrvID = ServerID });
			}
			if (RemoteSrv==null) return NetSessionResult.InvalidServerID;

			if ((RemoteSrv.CurrentPublicKeyExpirationTime <= Now) && (RemoteSrv.CurrentPublicKeyExpirationTime != 0))
				RemoteSrv.CurrentPublicKey = RemoteSrv.NextPublicKey;

			return Connect(RemoteSrv);
		}
		//-----------------------------------------------------------------------------------------------------


		public bool SendEncrypted()
		{
			//calculating and sending crc
			CRC Crc=new CRC();
			Crc.Update(SendBuff,0,(uint)SendBuff.Length);
			byte[] CrcBytes=BitConverter.GetBytes(Crc.GetDigest());
			NE.Encoder.EncryptBytes(CrcBytes, 0, CrcBytes.Length);
			if (!SendData(CrcBytes))
			{
				EndResult = NetSessionResult.NetError;
				return false;
			}

			//sending data
			NE.Encoder.EncryptBytes(SendBuff, 0, SendBuff.Length);
			if (!SendData())
			{
				EndResult = NetSessionResult.NetError;
				return false;
			}
			int R = Rnd.Next(0, 100);
			if (R >= PercentsOfNoise) return true;
			//sending trash data

			Rnd.NextBytes(CrcBytes);
			NE.Encoder.EncryptBytes(CrcBytes, 0, CrcBytes.Length);
			if (!SendData(CrcBytes))
			{
				EndResult = NetSessionResult.NetError;
				return false;
			}

			byte CmdCode = (byte)Rnd.Next(10, 60);
			int CmdSize = (SendBuff.Length + 128) * Rnd.Next(60, 135) / 100;
			byte[] Buff = new byte[CmdSize];
			Rnd.NextBytes(Buff);
			SendBuff = MakeCmd((CmdCode)CmdCode, Buff, CmdSize, 0);
			NE.Encoder.EncryptBytes(SendBuff, 0, SendBuff.Length);
			if (SendData()) return true;
			EndResult = NetSessionResult.NetError;
			return false;
		}
		//-----------------------------------------------------------------------------------------------------


		public bool ReceiveCommand() //true = continue, false = finish
		{
			if (!RecvData(8))
			{
				EndResult=NetSessionResult.NetError;
				return false;
			};
			NE.Decoder.EncryptBytes(RecvBuff, 0, 8);

			uint ReceivedCrcValue= BitConverter.ToUInt32(RecvBuff, 0);
			CRC Crc=new CRC();
			Crc.Update(RecvBuff,4,4);

			uint Cmd = BitConverter.ToUInt32(RecvBuff, 4);
			RecvCmdCode = (byte)(Cmd & 0xFF);
			RecvCmdSize = Cmd >> 8;

			if ((RecvCmdCode < 10) || (RecvCmdCode >= 60)) //do not check 'nothing' commands
			{
				if (!Enum.IsDefined(typeof (CmdCode), RecvCmdCode))
				{
					EndResult = NetSessionResult.InvalidData;
					return false;
				}
			}



			if (RecvCmdSize != 0)
			{
				if (!RecvData((int)RecvCmdSize))
				{
					EndResult = NetSessionResult.NetError;
					return false;
				};
				NE.Decoder.EncryptBytes(RecvBuff, 0, RecvBuff.Length);
				Crc.Update(RecvBuff,0,(uint)RecvBuff.Length);
			}
			if (ReceivedCrcValue != Crc.GetDigest()&&((RecvCmdCode < 10) || (RecvCmdCode >= 60)))
			{
				EndResult = NetSessionResult.InvalidData;
				return false;
			}

			return (RecvCmdCode < 10) || (RecvCmdCode >= 60) || ReceiveCommand(); //if CmdCode in (10,59) - skip data and receive next command
		}



		//-----------------------------------------------------------------------------------------------------

		protected void MakeHandshakeBytes(ushort VerHi, ushort VerLo,ulong EncryptionFlags, ulong EncryptionFlags2, long ServerID)
		{
			SendBuff=new byte[128];
			ParanoidRNG.GetBytes(SendBuff, 0,64);
			Blake512 Bl=new Blake512();

			Bl.TransformLong(ServerID);
			Bl.TransformBytes(SendBuff, 0,64);
			if ((PortPassword != null) && (PortPassword.Length >= 1)) Bl.TransformString(PortPassword);
			Bl.TransformLong(ServerID);

			byte[] tmp = (Bl.TransformFinal()).GetBytes();
			Bl.Initialize();

			Buffer.BlockCopy(tmp,0, SendBuff, 64,64);
			SendBuff[70] ^= (byte)'P';
			SendBuff[71] ^= (byte)'M';
			tmp= BitConverter.GetBytes(VerHi);
			SendBuff[72] ^= tmp[0];
			SendBuff[73] ^= tmp[1];
			tmp = BitConverter.GetBytes(VerLo);
			SendBuff[74] ^= tmp[0];
			SendBuff[75] ^= tmp[1];

			tmp = BitConverter.GetBytes(EncryptionFlags);
			for (int i = 0; i < 8; i++)
				SendBuff[80 + i] ^= tmp[i];

			tmp = BitConverter.GetBytes(EncryptionFlags2);
			for (int i = 0; i < 8; i++)
				SendBuff[90 + i] ^= tmp[i];



			//bytes 98-127 reserved for future use


		}

		protected bool CheckHandshakeData(long ServerID)
		{
			Blake512 Bl = new Blake512();
			Bl.TransformLong(ServerID);
			Bl.TransformBytes(RecvBuff,0,64);
			if ((PortPassword!=null)&&(PortPassword.Length>=1)) Bl.TransformString(PortPassword);
			Bl.TransformLong(ServerID);

			byte[] Hash= (Bl.TransformFinal()).GetBytes();
			for (int i = 0; i < 64; i++)
				RecvBuff[i + 64] ^= Hash[i];

			for (int i=64;i<70;i++)
				if (RecvBuff[i] != 0) return false;

			if ((RecvBuff[70] != (byte) 'P') || (RecvBuff[71] != (byte) 'M')) return false;

			UseVersionHi = BitConverter.ToUInt16(RecvBuff, 72);
			UseVersionLo = BitConverter.ToUInt16(RecvBuff, 74);
			SelectedRecvEncryptionType = BitConverter.ToUInt64(RecvBuff, 80);
			SelectedSendEncryptionType = BitConverter.ToUInt64(RecvBuff, 90);
			return true;
		}


		//-----------------------------------------------------------------------------------------------------
		public static void AfterSession(long RemoteSrvID, NetSessionResult NSR)
		{
			using (DB DBC=new DB())
			{

				if (NSR != NetSessionResult.Ok)
				{
					int FailedCallsCnt =
						DBC.Conn.QuerySingleOrDefault<int>("Select FailedCalls from Servers where ServerID=@ServerID",
							new {ServerID = RemoteSrvID});
					long HoldUntil = Now + Minutes((int)(Math.Pow(2, FailedCallsCnt)));
					++FailedCallsCnt;


					DBC.Conn.Execute("UPDATE Servers set FailedCalls=@FailedCalls, HoldUntil=@HoldTime,LastCallStatus=@CallStatus where ServerID=@ServerID",
					new {FailedCalls=FailedCallsCnt, HoldTime=HoldUntil, CallStatus = (int)NSR, ServerID = RemoteSrvID });


				}
				else
				{
					DBC.Conn.Execute("UPDATE Servers set FailedCalls=0, HoldUntil=0,LastCallStatus=@CallStatus where ServerID=@ServerID",
						new {CallStatus=(int)NSR,ServerID =  RemoteSrvID});

				}



			}

		}

		public void Dispose()
		{
			try
			{
				Sock.Close();
			}
			catch
			{
			}

			NE.Clear();

		}
	}

}


