using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Media;
using Dapper;
using System.Windows;
using Paranoid_Mail;

namespace Paranoid
{
	public class NetSessionClient : NetSessionExtended
	{
		private Account Acc;
		private bool isNewMailReceived = false;

		private long RealOutgoingMessageID;
		private long RealOutgoingUserID;
		private long RealOutgoingSrvID;
		private int RealOutgoungMsgType;


		public NetSessionResult CallServer(Account MyAcc)
		{
			Acc = MyAcc;
			Server Srv = Utils.GetServerInfo(Acc.ServerID);
			if (Srv == null) return NetSessionResult.InvalidData;
			{

				if ((Acc.OverrideIP != null) && (Acc.OverrideIP.Length >= 1)) Srv.IP = Acc.OverrideIP;
				if (Acc.OverridePort != 0) Srv.Port = Acc.OverridePort;
				PortPassword = Acc.PrivatePortPassword;

				NetSessionResult NSR=DataCall(Acc.ServerID, Acc.UserID, Srv, Acc.AuthPKey);
				if (isNewMailReceived) SystemSounds.Exclamation.Play();
				return NSR;
			}
		}



		protected override bool GetNextMessage()
		{
			using (DB DBC=new DB())
			{
				switch (DB.DatabaseType)
				{
					case DBType.SQLite:
					case DBType.MySQL:
						OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select * from Messages where MessageStatus=3 and FromServer=0 and FromUser=@AccountID Limit 1", Acc);
						break;
					case DBType.MSSQL:
						OutgoingMSG = DBC.Conn.QuerySingleOrDefault<Message>("Select TOP 1 * from Messages where MessageStatus=3 and FromServer=0 and FromUser=@AccountID", Acc);
						break;
				}

				if (OutgoingMSG == null) return false;
				byte[] DestPubAuthKey;

				OutgoingMSG.FromUser = Acc.UserID;
				OutgoingMSG.FromServer = Acc.ServerID;

				if (OutgoingMSG.ToUser != 0)
				{
					Contact Cnt = Acc.Contacts.SingleOrDefault(p => p.ContactID == OutgoingMSG.ToUser);
					if (Cnt == null)
					{
						DBC.Conn.Execute(
							"Update Messages set MessageStatus=255 where MessageStatus=3 and FromUser=@AccID and FromServer=0 and MessageID=@MsgID",
							new {AccID = Acc.AccountID, MsgID = OutgoingMSG.MessageID});
						return GetNextMessage();
					}
					OutgoingMSG.ToUser = Cnt.UserID;
					OutgoingMSG.ToServer = Cnt.ServerID;

					DestPubAuthKey = Cnt.RemoteAuthPublicKey;

					if ((OutgoingMSG.MessageType == (int) MsgType.MailMessage) ||
					    (OutgoingMSG.MessageType == (int) MsgType.FileAttachmentPart))
					{
						OutgoingMSG.MessageBody = Cnt.EncodeMsgForSending(OutgoingMSG.MessageBody);
						if (OutgoingMSG.MessageBody == null)
						{
							DBC.Conn.Execute(
								"Update Messages set MessageStatus=255 where MessageStatus=3 and FromUser=@AccID and FromServer=0 and MessageID=@MsgID",
								new {AccID = Acc.AccountID, MsgID = OutgoingMSG.MessageID});
							return GetNextMessage();
						}
					}



				}
				else
				{
					DestPubAuthKey = Utils.GetSrvPublicKey(OutgoingMSG.ToServer);
				}

				RealOutgoingMessageID = OutgoingMSG.MessageID;
				RealOutgoungMsgType = OutgoingMSG.MessageType;
				RealOutgoingSrvID = OutgoingMSG.ToServer;
				RealOutgoingUserID = OutgoingMSG.ToUser;

				switch ((MsgType) OutgoingMSG.MessageType)
				{
					case MsgType.MailMessage:
					case MsgType.FileAttachmentPart:
					case MsgType.AddContactAccepted:
					case MsgType.AddContactRefused:
					case MsgType.RequestAddContact:
					case MsgType.UserInfo:
					case MsgType.UserNotFound:
					case MsgType.MessageDelivered:
					case MsgType.RequestUserInfo:
					case MsgType.UpdateUserAuthKey:

						OutgoingMSG = SendRoutedMessage(OutgoingMSG.FromUser, OutgoingMSG.FromServer, OutgoingMSG.MessageID,
							OutgoingMSG.ToUser, OutgoingMSG.ToServer, OutgoingMSG.MessageType, OutgoingMSG.MessageBody, DestPubAuthKey);
						if (OutgoingMSG==null)
							DBC.Conn.Execute(
							   "Update Messages set MessageStatus=255 where MessageStatus=3 and FromUser=@AccID and FromServer=0 and MessageID=@MsgID",
							   new { AccID = Acc.AccountID, MsgID = RealOutgoingMessageID });
						break;



				}
				return true;
			}
		}

		protected override bool CheckMsgHeader(MessageHeader MsgHdr)
		{
			return (MsgHdr.MessageDataSize <= NetworkVariables.MaxMessageSize) && (MsgHdr.ToServer == Acc.ServerID) &&
			       (MsgHdr.ToUser == Acc.UserID) && (MsgHdr.MessageType == (int) MsgType.RoutedMessage);
		}

		protected override bool ProcessMessage(Message Msg)
		{
			if (!CheckRoutedMessage(Msg))
			{
				return true;
			}

			Message ReceivedMsg = DecodeRoutedMessage(Msg, Acc.AuthPKey);

			if (ReceivedMsg == null)
			{
				return true;
			}

			long FromUsr = ReceivedMsg.FromUser;
			long FromSrv = ReceivedMsg.FromServer;
			long MsgID = ReceivedMsg.MessageID;


			if (ProcessClientMessages(ReceivedMsg))
			{
				if ((ReceivedMsg.MessageType != (int) MsgType.MessageDelivered)&&(ReceivedMsg.FromUser!=0))
				{
					//send delivered message
					Contact Cnt = Acc.FindContact(FromUsr, FromSrv);
					if (Cnt != null)
					{
						Message DeliveredMsg = new Message
						{
							MessageType = (int) MsgType.MessageDelivered,
							FromUser=Acc.AccountID,
							FromServer=0,
							ToUser=	Cnt.ContactID,
							ToServer = Acc.AccountID,
							MessageID =Utils.MakeMsgID(),
							MessageStatus=(int)MsgStatus.Outbox,
							MessageBody= BitConverter.GetBytes(MsgID)

						};

						Message.SaveToDB(DeliveredMsg);
					}

				}

			};

			return true;


		}
		protected override bool isMessageAlreadyReceived(MessageHeader MH)
		{
			/*Contact Cnt = Acc.FindContact(MH.FromUser, MH.FromServer);
			if (Cnt == null) return false;
			using (DB DBC=new DB())
			{

				return (DBC.Conn.QuerySingleOrDefault<int>(
					"Select count(*) from Messages where FromUser=@CntID and FromServer=@AccID and MessageID=@MsgID and ToServer=0 and ToUser=@AccID",
					new {CntID = Cnt.ContactID, AccID = Acc.AccountID, MsgID = Cnt.TranslateMsgID(MH.MessageID)})!=0);
			} */
			return false;
		}

		protected override void MessageSentOk()
		{
			switch (OutgoingMSG.MessageType)
			{
				case (int) MsgType.DeleteUser:
					Acc.AccountDeletionDone();
					isFinished = true;
					break;
				case (int)MsgType.UpdateUserKeys:
					Acc.KeysUpdateDone();

					break;
			}
			using (DB DBC = new DB())
			{
				DBC.Conn.Execute(
					RealOutgoungMsgType != (int) MsgType.MessageDelivered
						? "Update Messages Set MessageStatus=5, RecvTime=@TimeNow where FromUser=@FromUsr and FromServer=0 and MessageID=@MsgID"
						: "Delete from Messages where FromUser=@FromUsr and FromServer=0 and MessageID=@MsgID",
					new {FromUsr = Acc.AccountID, MsgID = RealOutgoingMessageID,TimeNow=LongTime.Now});

			}

			if ((MsgType) (RealOutgoungMsgType) == MsgType.MailMessage)
			{
				Contact Cnt = Acc.FindContact(RealOutgoingUserID, RealOutgoingSrvID);
				if (Cnt != null) MainWindow.UpdateMessageStatus(Cnt, Acc.AccountID, 0, RealOutgoingMessageID, MsgStatus.Sent);
			}
		}

		protected override void MessageSentRejected()
		{
			using (DB DBC=new DB())
			{

				DBC.Conn.Execute(
					"Update Messages Set MessageStatus=6 where FromUser=@FromUsr and FromServer=0 and MessageID=@MsgID",
					new {FromUsr = Acc.AccountID, MsgID = RealOutgoingMessageID});
			}
			if ((MsgType)(OutgoingMSG.MessageType) == MsgType.MailMessage)
			{
				Contact Cnt = Acc.FindContact(RealOutgoingUserID, RealOutgoingSrvID);
				if (Cnt != null) MainWindow.UpdateMessageStatus(Cnt, Acc.AccountID, 0, RealOutgoingMessageID, MsgStatus.Rejected);
			}
		}


		private bool ProcessClientMessages(Message Msg)
		{
			switch ((MsgType)(Msg.MessageType))
			{
				case MsgType.MessageDelivered:
					if (Msg.MessageBody.Length != 8) return false;
					using (DB DBC = new DB())
					{
						long MessageID = BitConverter.ToInt64(Msg.MessageBody, 0);


						DBC.Conn.Execute(
							"Update Messages set MessageStatus=7 where MessageStatus=5 and FromUser=@FromUsr and FromServer=0 and MessageID=@MsgID",
							new
							{
								FromUsr = Acc.AccountID,
								MsgID = MessageID
							});

						long CntID = DBC.Conn.QueryFirstOrDefault<long>(
							"Select ToUser from Messages where MessageType=10 and FromUser=@FromUsr and FromServer=0 and MessageID=@MsgID",
							new
							{
								FromUsr = Acc.AccountID,
								MsgID = MessageID
							});
						if (CntID != 0)
						{
							Contact Cnt = Acc.Contacts.FirstOrDefault(p => p.ContactID == CntID);
							if (Cnt != null) MainWindow.UpdateMessageStatus(Cnt, Acc.AccountID, 0, MessageID, MsgStatus.Delivered);
						}
						return true;
					}



				case MsgType.MailMessage:
				case MsgType.FileAttachmentPart:
					{
						Contact Cnt = Acc.FindContact(Msg.FromUser, Msg.FromServer);
						Msg.FromServer = Acc.AccountID;
						Msg.FromUser = Cnt.ContactID;
						Msg.ToServer = 0;
						Msg.ToUser = Acc.AccountID;
						Msg.MessageID = Cnt.TranslateMsgID(Msg.MessageID);
						using (DB DBC = new DB())
						{
							if (DBC.Conn.QuerySingleOrDefault<int>("Select Count(*) from Messages where FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageID", Msg) != 0) return true;
						}
						//re-encode
						byte[] RecodedMsg = Cnt.RecodeReceivedMsg(Msg.MessageBody);
						if (RecodedMsg == null)
						{
							Msg.MessageStatus = (int)MsgStatus.Bad;
							Message.SaveToDB(Msg);
							return true;
						}

						Msg.MessageBody = RecodedMsg;

						Msg.MessageStatus = (int)MsgStatus.Received;
						Message.SaveToDB(Msg);
						if (Msg.MessageType == (int)MsgType.MailMessage)
						{
							Cnt.UnreadMessages += 1;
							MainWindow.AddMessageToList(Cnt, new MessageListItem(Msg));
							isNewMailReceived = true;
						}
						return true;
					}

				case MsgType.UserNotFound:
					return Contact.ContactNotFound(Acc, Msg);

				case MsgType.UserInfo:
					return Contact.ContactInfoReceived(Acc, Msg);

				case MsgType.AddContactRefused:
					{
						Contact.ProcessIncomingContactRefuse(Acc, Msg);
						return true;
					}

				case MsgType.AddContactAccepted:
					return Contact.ProcessIncomingContactAccept(Acc, Msg);

				case MsgType.RequestAddContact:
					return Contact.ProcessIncomingContactRequest(Acc, Msg);

				case MsgType.UpdateUserAuthKey:
					return Contact.ProcessUpdateUserAuthKey(Acc, Msg);

				default:
					return false;
			}
		}


	}

}
