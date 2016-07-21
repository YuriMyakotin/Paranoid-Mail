using System;
using System.Linq;

using Dapper;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Chaos.NaCl;
using HashLib.Crypto.SHA3;
using ProtoBuf;

namespace Paranoid
{
	public enum MsgSendStatus : int
	{
		NoSending = 0,
		HeaderSent = 1,
		SendingBody = 2,
		SendingComplete = 3
	}



	//-----------------------------------------------------------------------------------------------------
	public class NetSessionExtended : NetSession
	{


		private Queue<byte[]> SendingQueue;
		private object QueueLocker;

		private bool isMsgReceiving = false;
		private MsgSendStatus MsgSendingStatus = MsgSendStatus.NoSending;

		protected Message IncomingMSG, OutgoingMSG;

		private int IncomingMSGCurrentPos;
		private int OutgoingMSGCurrentPos;

		private ManualResetEvent HaveDataToSendEvent;

		protected volatile bool isFinished;

		private volatile bool isSendingDone;
		private volatile bool isReceivingDone;




		protected NetSessionResult DataExchangeProc()
		{
			SendingQueue = new Queue<byte[]>();
			QueueLocker = new object();
			HaveDataToSendEvent = new ManualResetEvent(true);
			isFinished = false;


			isSendingDone = false;
			isReceivingDone = false;


			Task ReceivingTask = new Task(ReceivingDataProc);
			ReceivingTask.Start();

			while (!isFinished)
			{

				bool isHaveDataToSend = false;
				lock (QueueLocker)
				{
					if (SendingQueue.Count != 0)
					{
						isHaveDataToSend = true;
						SendBuff = SendingQueue.Dequeue();
					}
				}
				if (!isHaveDataToSend)
				{
					switch (MsgSendingStatus)
					{
						case MsgSendStatus.NoSending:

							if (GetNextMessage())
							{
								if (OutgoingMSG.MessageBody.Length < 32000)
								{
									//send whole message
									SendBuff = MakeCmd<Message>(CmdCode.Message, OutgoingMSG);
									MsgSendingStatus = MsgSendStatus.SendingComplete;
								}
								else
								{
									OutgoingMSGCurrentPos = 0;
									MessageHeader Hdr = new MessageHeader(OutgoingMSG);
									SendBuff = MakeCmd<MessageHeader>(CmdCode.MessageHeader, Hdr);
									MsgSendingStatus = MsgSendStatus.HeaderSent;
								}
								isHaveDataToSend = true;
							}
							break;

						case MsgSendStatus.SendingBody:
						{
							int BlockSize = Rnd.Next(14000, 32000);

							if (OutgoingMSG.MessageBody.Length > OutgoingMSGCurrentPos + BlockSize)
							{
								SendBuff = MakeCmd(CmdCode.MessageDataPart, OutgoingMSG.MessageBody, BlockSize,
									OutgoingMSGCurrentPos);
								OutgoingMSGCurrentPos += BlockSize;
							}
							else
							{
								SendBuff = MakeCmd(CmdCode.MessageDataPart, OutgoingMSG.MessageBody,
									OutgoingMSG.MessageBody.Length - OutgoingMSGCurrentPos, OutgoingMSGCurrentPos);
								MsgSendingStatus = MsgSendStatus.SendingComplete;
							}
							isHaveDataToSend = true;
						}
							break;
					}
				}

				if (isHaveDataToSend)
				{
					isSendingDone = false;
					if (!SendEncrypted())
					{
						isFinished = true;
						EndResult = NetSessionResult.NetError;
					}
				}
				else
				{
					if (isSendingDone && isReceivingDone)
					{
						isFinished = true;

					}
					else
					{
						HaveDataToSendEvent.Reset();
						if (MsgSendingStatus == MsgSendStatus.NoSending)
						{
							SendBuff = MakeCmd(CmdCode.SendingDone);
							if (!SendEncrypted())
							{
								isFinished = true;
							}
							isSendingDone = true;
						}
						HaveDataToSendEvent.WaitOne(3000);
					}
					;
				}
			}


			ReceivingTask.Wait();
			SendingQueue.Clear();

			HaveDataToSendEvent.Close();
			if (isSendingDone && isReceivingDone) EndResult = NetSessionResult.Ok;


			return EndResult;
		}

		public NetSessionResult DataCall(long MySrvID, long MyUsrID, Server RemoteSrv, byte[] MyPrivateKey)
		{
			MyServerID = MySrvID;
			MyUserID = MyUsrID;
			RemoteServerID = RemoteSrv.ServerID;


			if ((RemoteSrv.CurrentPublicKeyExpirationTime <= LongTime.Now) && (RemoteSrv.CurrentPublicKeyExpirationTime != 0))
				RemoteSrv.CurrentPublicKey = RemoteSrv.NextPublicKey;

			EndResult = Connect(RemoteSrv);
			if (EndResult != NetSessionResult.Ok) goto SessionDoneLabel;
			// login
			SendBuff = MakeCmd(CmdCode.LoginRequest);
			if (!SendEncrypted()) goto SessionDoneLabel;
			if (!ReceiveCommand()) goto SessionDoneLabel;
			if ((RecvCmdCode != (int) CmdCode.LoginServerSalt) || (RecvCmdSize != 64))
			{
				EndResult = NetSessionResult.InvalidData;
				goto SessionDoneLabel;
			}
			{
				byte[] Buff = new byte[80];
				byte[] Tmp = BitConverter.GetBytes(MyServerID);
				Buffer.BlockCopy(Tmp, 0, Buff, 0, 8);
				Tmp = BitConverter.GetBytes(MyUserID);
				Buffer.BlockCopy(Tmp, 0, Buff, 8, 8);
				Tmp = Ed25519.Sign(RecvBuff, Ed25519.ExpandedPrivateKeyFromSeed(MyPrivateKey));
				Buffer.BlockCopy(Tmp, 0, Buff, 16, 64);
				SendBuff = MakeCmd(CmdCode.LoginCallerInfo, Buff, 80, 0);
			}
			if (!SendEncrypted()) goto SessionDoneLabel;
			if (!ReceiveCommand()) goto SessionDoneLabel;

			switch ((CmdCode) RecvCmdCode)
			{
				case CmdCode.Busy:
					EndResult = NetSessionResult.Busy;
					break;
				case CmdCode.LoginAccepted:
					EndResult = DataExchangeProc();
					break;

				default:
					EndResult = NetSessionResult.InvalidData;
					break;
			}


			SessionDoneLabel:
			AfterSession(RemoteServerID, EndResult);
			return EndResult;
		}

		protected void AddDataToQueue(byte[] Data)
		{
			lock (QueueLocker)
			{
				SendingQueue.Enqueue(Data);
				if (SendingQueue.Count == 1) HaveDataToSendEvent.Set();
			}
		}


		private void ReceivingDataProc()
		{
			while (!isFinished)
			{
				if (!ReceiveCommand())
				{
					isFinished = true;
					HaveDataToSendEvent.Set();
					return;
				}
				ProcessReceivedCommand();
			}
		}


		private void ProcessReceivedCommand()
		{
			if ((CmdCode) RecvCmdCode != CmdCode.SendingDone) isReceivingDone = false;

			switch ((CmdCode) RecvCmdCode)
			{
				case CmdCode.SendingDone:
					isReceivingDone = true;
					if (isReceivingDone && isSendingDone) isFinished = true;
					else HaveDataToSendEvent.Set();
					break;

				case CmdCode.Message:
				{
					Message Msg = Utils.BytesToObject<Message>(RecvBuff);
					if (Msg == null)
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}

					if (CheckMsgHeader(new MessageHeader(Msg)))
					{
						if (ProcessMessage(Msg))
						{
							AddDataToQueue(MakeCmd(CmdCode.MessageReceived));
							return;
						}
					}
					AddDataToQueue(MakeCmd(CmdCode.MessageRejected));
					return;
				}

				case CmdCode.MessageHeader:
				{
					if (isMsgReceiving)
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}

					MessageHeader MH = Utils.BytesToObject<MessageHeader>(RecvBuff);
					if (MH == null)
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}
					if (!CheckMsgHeader(MH))
					{
						AddDataToQueue(MakeCmd(CmdCode.MessageRejected));
						return;
					}



					if (!isMessageAlreadyReceived(MH))
					{
						IncomingMSG = new Message
						{
							FromUser = MH.FromUser,
							FromServer = MH.FromServer,
							MessageID = MH.MessageID,
							ToUser = MH.ToUser,
							ToServer = MH.ToServer,
							MessageType = MH.MessageType,
							MessageBody = new byte[MH.MessageDataSize]
						};
						IncomingMSGCurrentPos = 0;
						isMsgReceiving = true;
						AddDataToQueue(MakeCmd(CmdCode.MessageAccepted));
						return;
					}

					//already have that message
					AddDataToQueue(MakeCmd(CmdCode.MessageReceived));
					isMsgReceiving = false;
					return;
				}

				case CmdCode.MessageAccepted:
					if ((MsgSendingStatus != MsgSendStatus.HeaderSent) || (RecvCmdSize != 0))
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}

					MsgSendingStatus = MsgSendStatus.SendingBody;
					HaveDataToSendEvent.Set();
					return;

				case CmdCode.MessageRejected:
					if ((MsgSendingStatus != MsgSendStatus.HeaderSent)&&(MsgSendingStatus != MsgSendStatus.SendingComplete))
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}
					MessageSentRejected();

					MsgSendingStatus = MsgSendStatus.NoSending;
					HaveDataToSendEvent.Set();
					return;

				case CmdCode.MessageReceived:
					if ((MsgSendingStatus != MsgSendStatus.SendingComplete) &&
					    (MsgSendingStatus != MsgSendStatus.HeaderSent))
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}

					MessageSentOk();

					MsgSendingStatus = MsgSendStatus.NoSending;
					HaveDataToSendEvent.Set();
					return;


				case CmdCode.MessageDataPart:
					if (!isMsgReceiving)
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}
					if (IncomingMSGCurrentPos + RecvCmdSize > IncomingMSG.MessageBody.Length)
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
						return;
					}
					Buffer.BlockCopy(RecvBuff, 0, IncomingMSG.MessageBody, IncomingMSGCurrentPos, (int) RecvCmdSize);

					IncomingMSGCurrentPos += (int) RecvCmdSize;
					if (IncomingMSGCurrentPos == IncomingMSG.MessageBody.Length)
					{
						//finished receiving
						isMsgReceiving = false;
						AddDataToQueue(ProcessMessage(IncomingMSG)
							? MakeCmd(CmdCode.MessageReceived)
							: MakeCmd(CmdCode.MessageRejected));
					}
					return;

				default:
					if (!HandleOtherCommands())
					{
						EndResult = NetSessionResult.InvalidData;
						isFinished = true;
					}
					return;
			}
		}


		public static Message SendRoutedMessage(long FromUsr, long FromSrv, long MessageID, long ToUsr, long ToSrv,
			int MessageType, byte[] MsgData, byte[] DestPubAuthKey)
		{
			/*
				0	64	FromServer Sign
				64	32	FromServer hash with FromUserID
				96	32	PubKeyForDestServer
				128	8	ToUserID xor Hash  (SharedDestSrvKey and random)
				136	32	PubKeyForDestUser
				168	32	Header xor Hash	(SharedDestUserKey and random)
					168 8 FromUser
					176 8 ToUser
					184 8 MsgId
					192 4 MsgType
					196 4 Random bytes

				200	56	Random data
			*/
			if (DestPubAuthKey.Length != 32) return null;

			byte[] NewMsgData = new byte[MsgData.Length + 256];
			ParanoidRNG.GetBytes(NewMsgData, 196, 60);


			//make and encrypt message header
			byte[] RandomPrivateKey = new byte[32];

			ParanoidRNG.GetBytes(NewMsgData, 0, 96);

			ParanoidRNG.GetBytes(RandomPrivateKey, 0, 32);
			byte[] MyPubKey = Ed25519.PublicKeyFromSeed(RandomPrivateKey);
			Buffer.BlockCopy(MyPubKey, 0, NewMsgData, 136, 32);
			//
			byte[] SharedKey = Ed25519.KeyExchange(DestPubAuthKey, Ed25519.ExpandedPrivateKeyFromSeed(RandomPrivateKey));
			Skein256 SK = new Skein256();
			SK.TransformBytes(SharedKey);
			SK.TransformBytes(NewMsgData, 200, 56);
			SK.TransformBytes(SharedKey);
			byte[] Hash = (SK.TransformFinal()).GetBytes();
			SK.Initialize();

			byte[] tmp = BitConverter.GetBytes(FromUsr);
			Buffer.BlockCopy(tmp, 0, NewMsgData, 168, 8);

			tmp = BitConverter.GetBytes(ToUsr);
			Buffer.BlockCopy(tmp, 0, NewMsgData, 176, 8);

			tmp = BitConverter.GetBytes(MessageID);
			Buffer.BlockCopy(tmp, 0, NewMsgData, 184, 8);

			tmp = BitConverter.GetBytes(MessageType);
			Buffer.BlockCopy(tmp, 0, NewMsgData, 192, 4);


			for (int i = 0; i < 32; i++) NewMsgData[i + 168] ^= Hash[i];

			//make and encrypt part for remote server
			ParanoidRNG.GetBytes(RandomPrivateKey, 0, 32);
			MyPubKey = Ed25519.PublicKeyFromSeed(RandomPrivateKey);
			Buffer.BlockCopy(MyPubKey, 0, NewMsgData, 96, 32);

			SharedKey = Ed25519.KeyExchange(Utils.GetSrvPublicKey(ToSrv), Ed25519.ExpandedPrivateKeyFromSeed(RandomPrivateKey));
			SK.TransformBytes(SharedKey);
			SK.TransformBytes(NewMsgData, 200, 56);
			SK.TransformBytes(SharedKey);
			Hash = (SK.TransformFinal()).GetBytes();
			SK.Initialize();

			tmp =
				BitConverter.GetBytes(ToUsr ^ BitConverter.ToInt64(Hash, 0) ^ BitConverter.ToInt64(Hash, 8) ^
				                      BitConverter.ToInt64(Hash, 16) ^
				                      BitConverter.ToInt64(Hash, 24));
			Buffer.BlockCopy(tmp, 0, NewMsgData, 128, 8);


			Buffer.BlockCopy(MsgData, 0, NewMsgData, 256, MsgData.Length);

			for (int i = 0; i < SharedKey.Length; i++) SharedKey[i] = 0;
			for (int i = 0; i < RandomPrivateKey.Length; i++) RandomPrivateKey[i] = 0;


			return new Message()
			{
				FromUser=0,
				FromServer=FromSrv,
				MessageID = Utils.MakeMsgID(),
				ToUser =0,
				ToServer=ToSrv,
				MessageStatus = (int) MsgStatus.Outbox,
				MessageType = (int)MsgType.RoutedMessage,
				MessageBody = NewMsgData


			};

			//Message.PostMessage(0, FromSrv, 0, ToSrv, (int) MsgType.RoutedMessage, NewMsgData);

		}


		public static bool CheckRoutedMessage(Message RoutedMsg)
		{
			if (RoutedMsg.MessageBody.Length < 256) return false;

			byte[] RemoteSrvPubKey = Utils.GetSrvPublicKey(RoutedMsg.FromServer);
			if (RemoteSrvPubKey == null) return false;

			ArraySegment<byte> Sig = new ArraySegment<byte>(RoutedMsg.MessageBody, 0, 64);
			ArraySegment<byte> Body = new ArraySegment<byte>(RoutedMsg.MessageBody, 64, RoutedMsg.MessageBody.Length - 64);

			return Ed25519.Verify(Sig, Body, new ArraySegment<byte>(RemoteSrvPubKey));

		}


		public Message DecodeRoutedMessage(Message RoutedMsg,byte[] MyPvtAuthKey)
		{
			Message DecodedMsg = new Message
			{
				FromServer = RoutedMsg.FromServer,
				ToServer=RoutedMsg.ToServer,
				MessageStatus =(int) MsgStatus.Received
			};


			byte[] RemotePubKey = new byte[32];
			Buffer.BlockCopy(RoutedMsg.MessageBody,136,RemotePubKey,0,32);
			byte[] SharedKey = Ed25519.KeyExchange(RemotePubKey, Ed25519.ExpandedPrivateKeyFromSeed(MyPvtAuthKey));

			Skein256 SK = new Skein256();
			SK.TransformBytes(SharedKey);
			SK.TransformBytes(RoutedMsg.MessageBody, 200, 56);
			SK.TransformBytes(SharedKey);
			byte[] Hash = (SK.TransformFinal()).GetBytes();
			SK.Initialize();

			for (int i = 0; i < 32; i++) RoutedMsg.MessageBody[i + 168] ^= Hash[i];

			for (int i = 0; i < SharedKey.Length; i++) SharedKey[i] = 0;


			DecodedMsg.FromUser = BitConverter.ToInt64(RoutedMsg.MessageBody, 168);
			DecodedMsg.ToUser =BitConverter.ToInt64(RoutedMsg.MessageBody, 176);
			if (DecodedMsg.ToUser != MyUserID) return null;

			DecodedMsg.MessageID= BitConverter.ToInt64(RoutedMsg.MessageBody, 184);
			DecodedMsg.MessageType = BitConverter.ToInt32(RoutedMsg.MessageBody, 192);


			SK.TransformLong(DecodedMsg.FromUser);
			SK.TransformBytes(RoutedMsg.MessageBody, 200, 56);
			SK.TransformLong(DecodedMsg.FromUser);
			Hash = (SK.TransformFinal()).GetBytes();
			SK.Initialize();


			if (!CryptoBytes.ConstantTimeEquals(Hash, 0, RoutedMsg.MessageBody, 64, 32)) return null;

			DecodedMsg.MessageBody=new byte[RoutedMsg.MessageBody.Length-256];
			Buffer.BlockCopy(RoutedMsg.MessageBody,256,DecodedMsg.MessageBody,0, RoutedMsg.MessageBody.Length - 256);

			return DecodedMsg;
		}


	protected virtual bool CheckMsgHeader(MessageHeader MsgHdr) => false;
		protected virtual bool ProcessMessage(Message Msg) => false;

		protected virtual bool GetNextMessage() => false;

		protected virtual bool HandleOtherCommands() => false; // for possible extensions

		protected virtual bool isMessageAlreadyReceived(MessageHeader MH) => false;

		protected virtual void MessageSentOk()
		{}

		protected virtual void MessageSentRejected()
		{}
	}
}


