using System;

using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using Dapper;
using static Paranoid.Utils;
using static Paranoid.LongTime;

namespace Paranoid
{
	public partial class NetSessionServer : NetSessionExtended
	{
		protected override bool CheckMsgHeader(MessageHeader MsgHdr)
		{
			if (MsgHdr.MessageDataSize > NetworkVariables.MaxMessageSize) return false;

			if (RemoteNodeType == RemoteType.User)	  //messages from user
			{

				if (MsgHdr.FromServer != RemoteServerID) return false;

				if (MsgHdr.MessageType == (int) MsgType.RoutedMessage) return true;

				if (MsgHdr.FromUser != RemoteUserID) return false;


				switch ((MsgType) (MsgHdr.MessageType))
				{
					case MsgType.UpdateUserKeys:
					case MsgType.DeleteUser:
						return (MsgHdr.ToServer == MyServerID) && (MsgHdr.ToUser == 0);

					default:
						return false;
				}
			}

			//messages from server

			switch ((MsgType) (MsgHdr.MessageType))
			{

				case MsgType.RoutedMessage:
					return (Cfg.ServerInfo.isRelayingSupported ||((MsgHdr.ToServer == MyServerID)&&isServerExists(MsgHdr.FromServer)));

				case MsgType.UpdateServerInfo:
					return (Cfg.ServerInfo.isRootServer &&
						  ((MsgHdr.ToServer == MyServerID)||(MsgHdr.ToServer==0)));


				case MsgType.ServerRegistered:
				case MsgType.ServerRegistrationCheck:
				case MsgType.ServerRegistrationReplyNo:
				case MsgType.ServerRegistrationReplyOk:
					return (Cfg.ServerInfo.isRootServer && (MsgHdr.ToServer == MyServerID)&&(RemoteNodeType==RemoteType.RootServer));

				default:
					return false;
			}
		}

		protected override bool ProcessMessage(Message Msg)
		{
			if (Msg.MessageType==(int)MsgType.RoutedMessage)
			{
				//
				if (RemoteNodeType==RemoteType.User)
					SignRoutedMessage(Msg,RemoteUserID);



				if (Msg.ToServer == MyServerID)
				{
					if (!CheckRoutedMessage(Msg))
					{
						return false;
					}

					long ToUsr = RoutedMessageGetToUser(Msg);


					if (ToUsr != 0)
					{
						Msg.ToUser = ToUsr;
						Msg.MessageStatus=(int)MsgStatus.Outbox;
						Message.SaveToDB(Msg);
						return true;
					}



					Message DecodedMsg = DecodeRoutedMessage(Msg,MySecretKey);
					if (DecodedMsg == null)
					{
						return false;
					}


					switch ((MsgType) (DecodedMsg.MessageType))
					{
						case MsgType.RequestUserInfo:
							if (DecodedMsg.MessageBody.Length != 40) return false;
							long UserID = BitConverter.ToInt64(DecodedMsg.MessageBody, 0);
							byte[] RemoteAuthPubKey = new byte[32];
							Buffer.BlockCopy(DecodedMsg.MessageBody, 8, RemoteAuthPubKey, 0, 32);


							User UserInfo;
							byte[] DataToSend;
							int MsgTypeToSend;


							using (DB DBC = new DB())
							{

								UserInfo =
										DBC.Conn.QuerySingleOrDefault<User>("Select * from Users where UserID=@UsrID", new { UsrID = UserID });


							}
							if ((UserInfo == null) || (UserInfo.EncryptionKey.Length != 128) || (UserInfo.AuthKey.Length != 32))
							{

								MsgTypeToSend = (int) MsgType.UserNotFound;
								DataToSend=BitConverter.GetBytes(UserID);
							}
							else
							{
								DataToSend = new byte[168];
								Buffer.BlockCopy(DecodedMsg.MessageBody, 0, DataToSend, 0, 8);
								Buffer.BlockCopy(UserInfo.EncryptionKey, 0, DataToSend, 8, 128);
								Buffer.BlockCopy(UserInfo.AuthKey, 0, DataToSend, 136, 32);
								MsgTypeToSend = (int) MsgType.UserInfo;

							}

							Message NewMsg = SendRoutedMessage(0, MyServerID, MakeMsgID(), DecodedMsg.FromUser, DecodedMsg.FromServer,
								MsgTypeToSend, DataToSend, RemoteAuthPubKey);
							SignRoutedMessage(NewMsg,0);

							Message.SaveToDB(NewMsg);


							return true;


						default:
							return false;

					}

				}

				Msg.FromUser=0;
				Msg.ToUser += 1;
				Msg.MessageStatus = (int) MsgStatus.Outbox;
				Message.SaveToDB(Msg);
				return true;
			}

			//




			if ((Msg.ToServer == 0) && Cfg.ServerInfo.isRootServer) Msg.ToServer = MyServerID;




			if (Msg.FromUser != 0) //user messages
				switch ((MsgType)Msg.MessageType)
				{
					case MsgType.DeleteUser:
						using (DB DBC=new DB())
						{
							DBC.Conn.Execute(
								"Delete from Messages where (ToServer=@SrvID and ToUser=@UsrID) or (FromServer=@SrvID and FromUser=@UsrID)",
								new { SrvID = MyServerID, UsrID = Msg.FromUser });
							DBC.Conn.Execute("Delete from Users where UserID=@UsrID", new { UsrID = Msg.FromUser });
						}
						return true;

					case MsgType.UpdateUserKeys:
					{
						User Usr = BytesToObject<User>(Msg.MessageBody);
						if (Usr?.UserID != Msg.FromUser) return false;
						using (DB DBC=new DB())
						{
							DBC.Conn.Execute(
								"Update Users set AuthKey=@AuthKey,EncryptionKey=@EncryptionKey where UserID=@UserID",
								Usr);
						}
						return true;
					}

					default:
						return false;
				}

				//server messages
				switch ((MsgType)Msg.MessageType)
				{
					case MsgType.UpdateServerInfo:
						{
							long TimeNow =Now;

							UpdateServerInfo SrvInfo = BytesToObject<UpdateServerInfo>(Msg.MessageBody);
							if (SrvInfo == null)
							{
								return false;
							}
							if (SrvInfo.ServerID == Cfg.ServerInfo.ServerID) return false;
							if (SrvInfo.NextPublicKey.Length != 32) return false;
							if (SrvInfo.CurrentPublicKeyExpirationTime < TimeNow) return false;

							if (RemoteNodeType == RemoteType.NormalServer)
							{
								if (SrvInfo.ServerID != RemoteServerID) return false;
								if ((SrvInfo.ServerFlags & (int)ServerFlagBits.RootServer) != 0)
									return false; //only root server can promote another server to root

								if ((SrvInfo.Port <= 0) || (SrvInfo.Port > 65535)) return false;
								try
								{
									IPHostEntry Addr = Dns.GetHostEntry(SrvInfo.IP);
									if (Addr.AddressList.Length == 0) return false;
								}
								catch
								{
									return false;
								}
								//check done, send update to other root servers
								var RootServers = GetRootSrvsList();
								foreach (long SrvID in RootServers)
								{
									if (SrvID != Cfg.ServerInfo.ServerID)
										Message.PostMessage(0, Cfg.ServerInfo.ServerID, 0, SrvID,
											(int)MsgType.UpdateServerInfo, ObjectToBytes<UpdateServerInfo>(SrvInfo));
								}

							}

							using (DB DBC=new DB())
							{

								Server NewSrvInfo =
									DBC.Conn.QuerySingleOrDefault<Server>("Select * from Servers where ServerID=@SrvID",
										new { SrvID = SrvInfo.ServerID });
								if (NewSrvInfo == null) return false;

								if ((NewSrvInfo.CurrentPublicKeyExpirationTime <= TimeNow)&&(NewSrvInfo.NextPublicKey!=null))
								{
									NewSrvInfo.CurrentPublicKey = NewSrvInfo.NextPublicKey;
								}
								NewSrvInfo.NextPublicKey = SrvInfo.NextPublicKey;
								NewSrvInfo.Comments = SrvInfo.Comments;
								NewSrvInfo.CurrentPublicKeyExpirationTime = SrvInfo.CurrentPublicKeyExpirationTime;
								NewSrvInfo.IP = SrvInfo.IP;
								NewSrvInfo.Port = SrvInfo.Port;
								NewSrvInfo.ServerFlags = SrvInfo.ServerFlags;
								NewSrvInfo.ServerInfoTime = TimeNow;
								DBC.Conn.Execute(
										"Update Servers set ServerFlags=@ServerFlags,ServerInfoTime=@ServerInfoTime,IP=@IP,Port=@Port,CurrentPublicKey=@CurrentPublicKey,CurrentPublicKeyExpirationTime=@CurrentPublicKeyExpirationTime,NextPublicKey=@NextPublicKey,Comments=@Comments where ServerID=@ServerID",
										NewSrvInfo);
						}
							return true;
						}

					case MsgType.ServerRegistrationCheck:
						{
							if (RemoteNodeType != RemoteType.RootServer) return false;
							SrvRegCheck RegCheck = BytesToObject<SrvRegCheck>(Msg.MessageBody);

							if (RegCheck == null) return false;
							if (RegCheck.SrvID == 0) return false;

							int ReplyType = CheckRegistration(RegCheck)
								? (int) MsgType.ServerRegistrationReplyOk
								: (int) MsgType.ServerRegistrationReplyNo;
							Message.PostMessage(0, Cfg.ServerInfo.ServerID, 0, Msg.FromServer, ReplyType, BitConverter.GetBytes(RegCheck.SrvID));


							return true;
						}


					case MsgType.ServerRegistrationReplyOk:
						{
							if (RemoteNodeType != RemoteType.RootServer) return false;
							if (Msg.MessageBody.Length != 8) return false;
							long SrvID = BitConverter.ToInt64(Msg.MessageBody,0);
							using (DB DBC=new DB())
							{

								DBC.Conn.Execute("Update NewServerRegData set AwaitRepliesCount=AwaitRepliesCount-1 where NewSrvID=@ServerID and AwaitRepliesCount>0",
									new { ServerID = SrvID });
							}

							return true;
						}

					case MsgType.ServerRegistrationReplyNo:
						{
							if (RemoteNodeType != RemoteType.RootServer) return false;
							if (Msg.MessageBody.Length != 8) return false;
							long SrvID = BitConverter.ToInt64(Msg.MessageBody, 0);
							using (DB DBC=new DB())
							{

								DBC.Conn.Execute("Update NewServerRegData set AwaitRepliesCount=-1 where NewSrvID=@ServerID",
									new {ServerID = SrvID});
							}
							return true;
						}


					case MsgType.ServerRegistered:
						{
							if (RemoteNodeType != RemoteType.RootServer) return false;
							Server NewSrv = BytesToObject<Server>(Msg.MessageBody);
							if (NewSrv == null) return false;
							if (!ValidateServerInfo(NewSrv, Now)) return false;
							using (DB DBC=new DB())
							{
								if (DBC.Conn.QuerySingleOrDefault<int>("Select count(*) from Servers where ServerID=@SrvID", new {SrvID=NewSrv.ServerID}) != 0) return false;

								NewSrv.InsertIntoDB(DBC);
							}

							return true;
						}



					default:
						return false;
				}

		}
	}
}
