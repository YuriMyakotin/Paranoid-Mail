using System;
using ProtoBuf;

using Dapper;


namespace Paranoid
{
	public enum MsgType : int
	{
		RoutedMessage = 0,

		MailMessage =10,
		FileAttachmentPart=11,

		RequestUserInfo=50,
		UserInfo=51,
		UserNotFound=52,
		RequestAddContact=53,
		AddContactAccepted=54,
		AddContactRefused=55,

		UpdateUserAuthKey=58,

		MessageDelivered =70,

		UpdateUserKeys=110,

		DeleteUser=115,


		UpdateServerInfo=160,

		ServerRegistrationCheck=170,
		ServerRegistrationReplyOk=171,
		ServerRegistrationReplyNo=172,
		ServerRegistered=173
	}

	public enum MsgStatus : int
	{
		Received=1,
		Readed=2,

		Outbox=3,
		Sent=5,
		Rejected=6,
		Delivered=7,
		Bad=255
	}
	[ProtoContract] public class Message
	{
		[ProtoMember(1)] public long FromUser { get; set; }  //routed message = to Node
		[ProtoMember(2)] public long FromServer { get; set; }
		[ProtoMember(3)] public long MessageID { get; set; }
		[ProtoMember(4)] public long ToUser { get; set; } // routed message = hops count
		[ProtoMember(5)] public long ToServer { get; set; }
		[ProtoMember(6)] public int MessageType { get; set; }
		public int MessageStatus { get; set; }
		public long RecvTime { get; set; }
		[ProtoMember(7)] public byte[] MessageBody { get; set; }

		public static long PostMessage(long FromUsr, long FromSrv, long ToUsr, long ToSrv, int MsgType, byte[] MsgData)
		{
			Message MSG = new Message
			{
				FromUser = FromUsr,
				FromServer = FromSrv,
				ToUser = ToUsr,
				ToServer = ToSrv,
				MessageType = MsgType,
				MessageBody=MsgData,
				MessageStatus=(int)MsgStatus.Outbox,
				MessageID=Utils.MakeMsgID()
			};
			SaveToDB(MSG);
			return MSG.MessageID;
		}

		public static void SaveToDB(Message MSG)
		{
			MSG.RecvTime = LongTime.Now;
			using (DB DBC=new DB())
			{
				switch (DB.DatabaseType)
				{
					case DBType.SQLite:
					case DBType.MySQL:
						DBC.Conn.Execute("Replace into Messages values(@FromUser,@FromServer,@MessageID,@ToUser,@ToServer,@MessageType,@MessageStatus,@RecvTime,@MessageBody)",MSG);
						break;
					case DBType.MSSQL:
						DBC.Conn.Execute("Delete from Messages where FromUser=@FromUser and FromServer=@FromServer and MessageID=@MessageID; Insert into Messages values(@FromUser,@FromServer,@MessageID,@ToUser,@ToServer,@MessageType,@MessageStatus,@RecvTime,@MessageBody)", MSG);
						break;

				}
			}
		}
	}

	[ProtoContract] public class MessageHeader
	{
		[ProtoMember(1)] public long FromUser { get; set; }
		[ProtoMember(2)] public long FromServer { get; set; }
		[ProtoMember(3)] public long MessageID { get; set; }
		[ProtoMember(4)] public long ToUser { get; set; }
		[ProtoMember(5)] public long ToServer { get; set; }
		[ProtoMember(6)] public int MessageType { get; set; }
		[ProtoMember(7)] public int MessageDataSize { get; set; }

		public MessageHeader()
		{

		}
		public MessageHeader(Message Msg)
		{
			FromUser = Msg.FromUser;
			FromServer = Msg.FromServer;
			MessageID = Msg.MessageID;
			ToUser = Msg.ToUser;
			ToServer = Msg.ToServer;
			MessageType = Msg.MessageType;
			MessageDataSize = Msg.MessageBody.Length;
		}
	}



}
