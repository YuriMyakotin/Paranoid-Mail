using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dapper;
using static Paranoid.Utils;

namespace Paranoid
{
	public enum RemoteType : int
	{
		None = 0,
		User = 1,
		NormalServer = 2,
		RootServer = 3
	}


	public partial class NetSessionServer : NetSessionExtended
	{
		private static readonly List<long> BusyList = new List<long>();
		private static readonly object BusyListLock = new object();
		private readonly byte[] MySecretKey;

		private RemoteType RemoteNodeType;

		public AutoRegistrationModes RegistrationMode;

		private long AddrHash;

		public NetSessionServer() : base()
		{
			Cfg.CheckKeyExpiration();
			lock (Cfg.LockObj)
			{
				MySecretKey = Cfg.CurrentPrivateKey;
				MyServerID = Cfg.ServerInfo.ServerID;
			}
			MyUserID = 0;
			RemoteNodeType = RemoteType.None;
		}


		public static void AddBusy(long SrvID)
		{
			lock (BusyListLock)
			{
				BusyList.Add(SrvID);
			}
		}

		public static bool CheckBusy(long SrvID)
		{
			int idx;
			lock (BusyListLock)
			{
				idx = BusyList.IndexOf(SrvID);
			}
			return idx != -1;
		}

		public static void RemoveBusy(long SrvID)
		{
			lock (BusyListLock)
			{
				BusyList.Remove(SrvID);
			}
		}

		public static bool isAnyBusyNow()
		{
			lock (BusyListLock)
			{
				return (BusyList.Count != 0);
			}
		}



		public NetSessionResult Answer()
		{

			Sock = TcpClnt.Client;
			Sock.ReceiveTimeout = NetworkVariables.SocketTimeout;
			Sock.SendTimeout =  NetworkVariables.SocketTimeout;
			Sock.Blocking = true;
			Sock.ReceiveBufferSize = NetworkVariables.SocketBufferSize;
			Sock.SendBufferSize = NetworkVariables.SocketBufferSize;


			AddrHash = (long)Str2Hash.StringToHash(((IPEndPoint)Sock.RemoteEndPoint).Address.ToString());

			if (DynamicBlackList.CheckIP(AddrHash))
			{
				return NetSessionResult.NetError;
			}

			//stage 1 - sending PT signature, version, flags
			if (!RecvData(128))
			{
				DynamicBlackList.AddIP(AddrHash);
				return NetSessionResult.NetError;
			}

			NE.AddHashData(RecvBuff);
			if (!CheckHandshakeData(MyServerID))
			{
				DynamicBlackList.AddIP(AddrHash);
				return NetSessionResult.InvalidData;
			}

			if ((UseVersionHi*65536 + UseVersionLo) > (SupportedVersionHi*65536 + SupportedVersionLo))
			{
				UseVersionHi = SupportedVersionHi;
				UseVersionLo = SupportedVersionLo;
			}

			ulong SupportedEncryptionTypes=	SelectedRecvEncryptionType & SupportedEncryptionFlags;

			if (SupportedEncryptionTypes == 0) return NetSessionResult.InvalidData;

			while (true)
			{
				int i = Rnd.Next(0, 64);
				if ((SupportedEncryptionTypes & ((ulong) 1 << i)) == 0) continue;
				SelectedRecvEncryptionType = (ulong) 1 << i;
				break;
			}

			while (true)
			{
				int i = Rnd.Next(0, 64);
				if ((SupportedEncryptionTypes & ((ulong) 1 << i)) == 0) continue;
				SelectedSendEncryptionType = (ulong)1 << i;
				break;
			}


			MakeHandshakeBytes(UseVersionHi, UseVersionLo,SelectedSendEncryptionType,SelectedRecvEncryptionType,MyServerID);

			if (!SendData()) return NetSessionResult.NetError;
			NE.AddHashData(SendBuff);

			//stage 2 - receiving and sending public keys, signing data
			NE.Init(SelectedSendEncryptionType,SelectedRecvEncryptionType);

			if (!RecvData(NetworkEncryption.KeyBlockSize)) return NetSessionResult.NetError;
			NE.AddHashData(RecvBuff);
			SendBuff = NE.MakePublicKeysBlock();

			NE.SetSharedKey(RecvBuff,false);

			if (!RecvData(64)) return NetSessionResult.NetError;
			NE.AddHashData(RecvBuff);

			if (!SendData()) return NetSessionResult.NetError;
			NE.AddHashData(SendBuff);


			SendBuff = NE.MakeSignature(MySecretKey);
			if (!SendData()) return NetSessionResult.NetError;

			//stage 3 - process commands


			if (!ReceiveCommand())
			{
				return EndResult;
			}

			switch ((CmdCode) RecvCmdCode)
			{
				case CmdCode.LoginRequest:
				{
					if (RecvCmdSize != 0) return NetSessionResult.InvalidData;

					byte[] Salt = new byte[64];
					byte[] RemotePublicKey;
					ParanoidRNG.GetBytes(Salt);
					SendBuff = MakeCmd(CmdCode.LoginServerSalt, Salt, 64, 0);

					if (!SendEncrypted()) return NetSessionResult.NetError;

					if (!ReceiveCommand()) return EndResult;

					if (((CmdCode) RecvCmdCode != CmdCode.LoginCallerInfo) || (RecvCmdSize != 80))
						return NetSessionResult.InvalidData;

					RemoteServerID = BitConverter.ToInt64(RecvBuff, 0);
					RemoteUserID = BitConverter.ToInt64(RecvBuff, 8);
					byte[] Sig =new byte[64];
					Buffer.BlockCopy(RecvBuff,16,Sig,0,64);

					if (RemoteUserID == 0)
					{
						if (RemoteServerID == MyServerID) return NetSessionResult.InvalidCredentials;

						Server RemoteSrv = GetServerInfo(RemoteServerID);
						if (RemoteSrv == null)
						{
							DynamicBlackList.AddIP(AddrHash);
							return NetSessionResult.InvalidCredentials;
						}
						RemotePublicKey = GetSrvPublicKey(RemoteSrv);

						RemoteNodeType = RemoteSrv.isRootServer ? RemoteType.RootServer : RemoteType.NormalServer;
					}
					else
					{
						using (DB DBC=new DB())
						{
							User RemoteUser = DBC.Conn.QuerySingleOrDefault<User>("Select * from Users where UserID=@UsrID",new {UsrID=RemoteUserID});

							if (RemoteUser == null)
							{
									DynamicBlackList.AddIP(AddrHash);
									return NetSessionResult.InvalidCredentials;
							}
							RemoteNodeType = RemoteType.User;

							RemotePublicKey = RemoteUser.AuthKey;
						}
					}
					if (Chaos.NaCl.Ed25519.Verify(Sig, Salt, RemotePublicKey))
					{
						if (RemoteNodeType != RemoteType.User)
						{
							if (!CheckBusy(RemoteServerID))
								AddBusy(RemoteServerID);
							else
							{
								SendBuff = MakeCmd(CmdCode.Busy);
								return !SendEncrypted() ? NetSessionResult.NetError : NetSessionResult.Busy;
							}
						}
						SendBuff = MakeCmd(CmdCode.LoginAccepted);
						if (!SendEncrypted())
						{
							RemoveBusy(RemoteServerID);
							return NetSessionResult.NetError;
						}
						EndResult = DataExchangeProc();

						if (RemoteNodeType != RemoteType.User)
						{
							RemoveBusy(RemoteServerID);
						}
						return EndResult;
					}
					DynamicBlackList.AddIP(AddrHash);
					return NetSessionResult.InvalidCredentials;

				}


				case CmdCode.ServersListRequest:
					if (RecvCmdSize != sizeof (ulong)) return NetSessionResult.InvalidData;
					SendServersList();
					return NetSessionResult.Ok;


				case CmdCode.UserRegistrationRequest:
					return RegisterUser();


				case CmdCode.SrvRegistrationRequest:
					return RegisterServer();

				case CmdCode.SrvRegistrationCheck:
					return ServerRegCheck();

				default:
					return NetSessionResult.InvalidData;
			}
		}

		public NetSessionResult DataCall(Server Srv)
		{
			RemoteNodeType = Srv.isRootServer ? RemoteType.RootServer : RemoteType.NormalServer;
			return DataCall(Cfg.ServerInfo.ServerID, 0, Srv, Cfg.CurrentPrivateKey);
		}

		private void SendServersList()
		{

			long RequestedTime = BitConverter.ToInt64(RecvBuff, 0);
			using (DB DBC=new DB())
			{

				long CurrentMaxId = long.MinValue;

				switch (DB.DatabaseType)
				{
					case DBType.MySQL:
					case DBType.SQLite:

						while (true)
						{
							List<Server> ServersList = DBC.Conn.Query<Server>("Select * from Servers where ServerInfoTime>@ReqTime and ServerID>@PrevMaxID order by ServerID limit 512",
									   new { ReqTime = RequestedTime, PrevMaxID = CurrentMaxId }).ToList();

							if (ServersList.Count == 0) break;

							CurrentMaxId = ServersList.Select(p => p.ServerID).Max();
						    SendBuff = MakeCmd<List<Server>>(CmdCode.ServersListPart, ServersList);
							if (!SendEncrypted()) return;

						}
						break;

					case DBType.MSSQL:

						while (true)
						{
							List<Server> ServersList =DBC.Conn.Query<Server>("Select TOP 512 * from Servers where ServerInfoTime>@ReqTime and ServerID>@PrevMaxID order by ServerID",
									new { ReqTime = RequestedTime, PrevMaxID = CurrentMaxId }).ToList();

							if (ServersList.Count == 0) break;

							CurrentMaxId = ServersList.Select(p => p.ServerID).Max();
							SendBuff = MakeCmd<List<Server>>(CmdCode.ServersListPart, ServersList);
							if (!SendEncrypted()) return;

						}
						break;


				}

				SendBuff = MakeCmd(CmdCode.SendingDone);
				SendEncrypted();

			}

		}

		protected override bool GetNextMessage()
		{
			using (DB DBC=new DB())
			{

				OutgoingMSG = null;
				if (RemoteNodeType == RemoteType.User)
				{
					switch (DB.DatabaseType)
					{
						case DBType.SQLite:
						case DBType.MySQL:
							OutgoingMSG =DBC.Conn.QuerySingleOrDefault<Message>("Select * from Messages where MessageStatus=3 and ToUser=@ToUser and ToServer=@ToServer Limit 1",new { ToUser = RemoteUserID, ToServer = RemoteServerID });
							break;

						case DBType.MSSQL:
							OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select TOP 1 * from Messages where MessageStatus=3 and ToUser=@ToUser and ToServer=@ToServer", new { ToUser = RemoteUserID, ToServer = RemoteServerID });
							break;
					}
				}
				else
				{
					if (RemoteNodeType == RemoteType.RootServer)
					{
						switch (DB.DatabaseType)
						{
							case DBType.SQLite:
							case DBType.MySQL:
								OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select * from Messages where MessageStatus=3 and ToServer=0 and MessageType<>0 Limit 1");
								break;
							case DBType.MSSQL:
								OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select TOP 1 * from Messages where MessageStatus=3 and ToServer=0 and MessageType<>0");
								break;
						}
						if (OutgoingMSG != null) return true;
					}

					switch (DB.DatabaseType)
					{
						case DBType.SQLite:
						case DBType.MySQL:
							OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select * from Messages where MessageStatus=3 and ToServer=@ToServer and MessageType<>0 Limit 1",new { ToServer = RemoteServerID });
							break;

						case DBType.MSSQL:
							OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select TOP 1 * from Messages where MessageStatus=3 and ToServer=@ToServer and MessageType<>0",new { ToServer = RemoteServerID });
							break;
					}
					if (OutgoingMSG != null) return true;

					switch (DB.DatabaseType)
					{
						case DBType.SQLite:
						case DBType.MySQL:
							OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select * from Messages where MessageStatus=3 and FromUser=@ToServer and MessageType=0 Limit 1", new { ToServer = RemoteServerID });
							break;

						case DBType.MSSQL:
							OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select TOP 1 * from Messages where MessageStatus=3 and FromUser=@ToServer and MessageType=0", new { ToServer = RemoteServerID });
							break;
					}
				}
			}
			return OutgoingMSG != null;
		}


		protected override bool isMessageAlreadyReceived(MessageHeader MH)
		{
			using (DB DBC=new DB())
			{
				return (DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Messages where FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageID",MH)!=0);
			}
		}

		protected override void MessageSentOk()
		{
			using (DB DBC=new DB())
			{
				DBC.Conn.Execute(
					"Delete from Messages where FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageID",
					OutgoingMSG);
			}
		}

		protected override void MessageSentRejected()
		{
			using (DB DBC=new DB())
			{

				DBC.Conn.Execute(
					"Update Messages Set MessageStatus=6 where FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageID",
					OutgoingMSG);

			}
		}





		private static bool isUserExists(long UserID)
		{
			using (DB DBC=new DB())
			{

				return DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Users where UserID=@UID",
					new {UID = UserID}) != 0;
			}
		}


	}
}
