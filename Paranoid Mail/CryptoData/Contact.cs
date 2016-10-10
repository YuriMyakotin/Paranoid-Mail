using ProtoBuf;
using System.ComponentModel;
using System;
using Chaos.NaCl;
using System.Text;
using System.Windows;
using Dapper;


namespace Paranoid
{
	public enum ContactStatus : byte
	{
		Estabilished = 0,
		InfoRequested = 1,
		UserNotFound = 2,
		RequestSent = 3,
		RequestRejected = 4,
		OtherSideRequested = 5,
		Blocked = 255

	}

	[ProtoContract]	public class Contact : INotifyPropertyChanged
	{
		[ProtoMember(1)] public long ContactID { get; set; }
		[ProtoMember(2)] public long ServerID { get; set; }
		[ProtoMember(3)] public long UserID { get; set; }
		[ProtoMember(4)] public string Name { get; set; }
		[ProtoMember(5)] public ContactStatus Status { get; set; }
		[ProtoMember(6)] public string Comments { get; set; }
		[ProtoMember(7)] public byte[] MyPrivateKey { get; set; }
		[ProtoMember(8)] public byte[] OtherSidePublicKey { get; set; }
		[ProtoMember(9)] public byte[] SharedKey { get; set; }
		[ProtoMember(10)] public byte[] StorageKey { get; set; }
		[ProtoMember(11)] public byte[] RemoteAuthPublicKey { get; set; }
		[ProtoMember(12)] public byte[] MsgIdTranslationBytes { get; set; }
		[ProtoMember(13)] public byte[] ExtraData { get; set; }

		public Account ParentAccount;

		public string ContactAddress => ((ulong) UserID).ToString() + "@" + ((ulong) ServerID).ToString();

		public string ContactName
		{
			get { return ((Name == null) || (Name.Length == 0)) ? ContactAddress : Name; }
			set
			{
				Name = value;
				OnPropertyChanged("ContactName");
			}
		}

		public string ContactComments => (Comments == null) || (Comments.Length == 0) ? ContactAddress : Comments;

		private int _UnreadMessages = 0;
		public int UnreadMessages
		{
			get { return _UnreadMessages; }
			set
			{
				_UnreadMessages = value;
				OnPropertyChanged("UnreadMessages");
			}
		}

		public ContactStatus StatusOfContact
		{
			get { return Status; }
			set
			{
				Status = value;
				OnPropertyChanged("StatusOfContact");
			}
		}

		public Contact Self => this;

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		public static string StatusToStr(ContactStatus Status)
		{
			switch (Status)
			{
				case ContactStatus.InfoRequested:
					return "Requested user info";
				case ContactStatus.Estabilished:
					return "Estabilished";
				case ContactStatus.UserNotFound:
					return "User not found";
				case ContactStatus.Blocked:
					return "Blocked";
				case ContactStatus.RequestSent:
					return "Add contact request sent";
				case ContactStatus.RequestRejected:
					return "Other side refused contact request";
				case ContactStatus.OtherSideRequested:
					return "Incoming contact request";
				default:
					return string.Empty;
			}
		}


		public static void RequestContactInfo(Account Acc,long SrvID, long UsrID, string CntName, string Cmts, string MessageToContact, string RandomStr)
		{
			if ((SrvID == 0) || (UsrID == 0)) return;
			if (Acc.FindContact(UsrID, SrvID) != null) return;
			if (MessageToContact.Length > 64) return;

			Contact Cnt = new Contact
			{
				StatusOfContact = ContactStatus.InfoRequested,
				ContactID = Acc.MakeNewContactID(),
				ParentAccount = Acc,
				ServerID = SrvID,
				UserID = UsrID,
				Name = CntName,
				Comments = Cmts,
				MyPrivateKey = new byte[128],
				SharedKey = new byte[128],
				StorageKey = new byte[128],
				MsgIdTranslationBytes = new byte[16],
				ExtraData = Encoding.UTF8.GetBytes(MessageToContact)

			};
			//
			byte[] RndBytes = Encoding.UTF8.GetBytes(RandomStr);

			ParanoidRNG.GetBytes(Cnt.MyPrivateKey, 0, 128, RndBytes);
			ParanoidRNG.GetBytes(Cnt.SharedKey, 0, 128, RndBytes);
			ParanoidRNG.GetBytes(Cnt.StorageKey, 0, 128, RndBytes);
			ParanoidRNG.GetBytes(Cnt.MsgIdTranslationBytes, 0, 16, RndBytes);


			Acc.Contacts.Add(Cnt);
			CryptoData.SaveKeys();
			byte[] MessageData=new byte[40];
			byte[] tmp = BitConverter.GetBytes(UsrID);
			Buffer.BlockCopy(tmp,0,MessageData,0,8);
			tmp = Ed25519.PublicKeyFromSeed(Acc.AuthPKey);
			Buffer.BlockCopy(tmp, 0, MessageData, 8, 32);

			Message.PostMessage(Acc.AccountID,0,0,SrvID,(int)MsgType.RequestUserInfo,MessageData);
			BackgroundTasks.SendReceiveAccount(Acc);
		}
		public static bool ContactInfoReceived(Account Acc, Message Msg)
		{

			if (Msg.MessageBody.Length != 168) return false;
			Contact Cnt = Acc.FindContact(BitConverter.ToInt64(Msg.MessageBody, 0), Msg.FromServer);
			if (Cnt?.Status != ContactStatus.InfoRequested) return false;


			byte[] RemotePubKey= new byte[128];
			Buffer.BlockCopy(Msg.MessageBody, 8, RemotePubKey, 0, 128);
			Cnt.RemoteAuthPublicKey=new byte[32];
			Buffer.BlockCopy(Msg.MessageBody, 136, Cnt.RemoteAuthPublicKey, 0, 32);

			byte[] ReqBytes = Utils.ObjectToBytes(new AddContactRequestData
			{
				MessageToContact = Cnt.ExtraData,
				PublicKey = CryptoData.MakePublicKey1024(Cnt.MyPrivateKey),
				SharedKey = CryptoData.MakePublicKey1024(Cnt.SharedKey),
				AuthPubKey=Ed25519.PublicKeyFromSeed(Acc.AuthPKey)
			});


			byte[] SendingKey = CryptoData.MakeSharedKey1024(Acc.EncryptionPKey, RemotePubKey);

			byte[] EncryptedData = ParanoidHelpers.MakeEncryptedMessage(SendingKey, ReqBytes);
			for (int i = 0; i < 128; i++) SendingKey[i] = 0;

			byte[] MessageData = new byte[128 + EncryptedData.Length];
			byte[] MyPubKey = CryptoData.MakePublicKey1024(Acc.EncryptionPKey);

			Buffer.BlockCopy(MyPubKey, 0, MessageData, 0, 128);
			Buffer.BlockCopy(EncryptedData, 0, MessageData, 128, EncryptedData.Length);

			Cnt.ExtraData = RemotePubKey;
			Cnt.StatusOfContact = ContactStatus.RequestSent;

			CryptoData.SaveKeys();

			Message.PostMessage(Acc.AccountID, 0, Cnt.ContactID, Acc.AccountID, (int)MsgType.RequestAddContact, MessageData);
			return true;
		}
		public static bool ContactNotFound(Account Acc, Message Msg)
		{
			if (Msg.MessageBody.Length != 8) return false;
			Contact Cnt = Acc.FindContact(BitConverter.ToInt64(Msg.MessageBody, 0), Msg.FromServer);
			if (Cnt?.Status != ContactStatus.InfoRequested) return false;
			Cnt.StatusOfContact = ContactStatus.UserNotFound;
			CryptoData.SaveKeys();
			return true;
		}


		public static bool ProcessIncomingContactRequest(Account Acc, Message Msg)
		{
			if (Msg.MessageBody.Length < 512) return false;
			if ((Msg.FromServer == Acc.ServerID) && (Msg.FromUser == Acc.UserID)) return false;

			Contact Cnt = Acc.FindContact(Msg.FromUser, Msg.FromServer);
			if (Cnt != null)
			{
				switch (Cnt.Status)
				{
					case ContactStatus.InfoRequested:
					case ContactStatus.UserNotFound:
					case ContactStatus.RequestRejected:
						//delete own request, process incoming one
						break;


					case ContactStatus.Blocked:
					case ContactStatus.Estabilished:
					case ContactStatus.OtherSideRequested:
						return false; //ignore blocked or duplicate request

					case ContactStatus.RequestSent: //requests collision, decide - process or ignore.
						if (Cnt.UserID < Acc.UserID) return false;
						if ((Cnt.UserID == Acc.UserID) && (Cnt.ServerID < Acc.ServerID)) return false;
						break;

				}
				Cnt.DeleteContact(false);
			}


			Cnt = new Contact
			{
				StatusOfContact = ContactStatus.OtherSideRequested,
				ContactID = Acc.MakeNewContactID(),
				ParentAccount = Acc,
				ServerID = Msg.FromServer,
				UserID = Msg.FromUser
			};

			byte[] RemotePublicKey=new byte[128];
			Buffer.BlockCopy(Msg.MessageBody,0,RemotePublicKey,0,128);
			byte[] SharedKey = CryptoData.MakeSharedKey1024(Acc.EncryptionPKey, RemotePublicKey);
			if (SharedKey == null) return false;
			byte[] Data=new byte[Msg.MessageBody.Length-128];
			Buffer.BlockCopy(Msg.MessageBody,128,Data,0,Data.Length);

			byte[] DecryptedMessage = ParanoidHelpers.DecryptMessage(SharedKey, Data);
			for (int i = 0; i < 128; i++) SharedKey[i] = 0;
			if (DecryptedMessage == null)
			{
				return false;
			}

			AddContactRequestData ReqData = Utils.BytesToObject<AddContactRequestData>(DecryptedMessage);
			for (int i = 0; i < DecryptedMessage.Length; i++) DecryptedMessage[i] = 0;

			try
			{
				Cnt.Comments = Encoding.UTF8.GetString(ReqData.MessageToContact);
			}
			catch
			{
				return false;
			}
			if (ReqData?.PublicKey.Length != 128 || (ReqData.SharedKey.Length != 128) ||
				(Cnt.Comments.Length > 64)) return false;

			Cnt.OtherSidePublicKey = ReqData.PublicKey;
			Cnt.SharedKey = ReqData.SharedKey;
			Cnt.RemoteAuthPublicKey = ReqData.AuthPubKey;

			Application.Current.Dispatcher.Invoke(() => Acc.Contacts.Add(Cnt));


			CryptoData.SaveKeys();
			return true;
		}
		public static void ProcessIncomingContactRefuse(Account Acc, Message Msg)
		{
			Contact Cnt = Acc.FindContact(Msg.FromUser, Msg.FromServer);
			if (Cnt?.Status != ContactStatus.RequestSent) return;

			Cnt.StatusOfContact = ContactStatus.RequestRejected;
			CryptoData.SaveKeys();
		}

		public void IncomingContactRequestRefused()
		{
			Message.PostMessage(ParentAccount.AccountID,0,ContactID,ParentAccount.AccountID,(int)MsgType.AddContactRefused,new byte[1]);

			SharedKey = null;
			OtherSidePublicKey = null;
			StatusOfContact=ContactStatus.Blocked;

			CryptoData.SaveKeys();
			BackgroundTasks.SendReceiveAccount(ParentAccount);
		}

		public void IncomingContactRequestAccepted(string RandomStr)
		{
			if (Status != ContactStatus.OtherSideRequested) return;
			if ((OtherSidePublicKey.Length != 128) || (SharedKey.Length != 128)) return;

			byte[] RndBytes = Encoding.UTF8.GetBytes(RandomStr);

			byte[] SendingReplyKey = CryptoData.MakeSharedKey1024(ParentAccount.EncryptionPKey, OtherSidePublicKey);

			byte[] DataToSend = new byte[256];


			MyPrivateKey= new byte[128];
			ParanoidRNG.GetBytes(MyPrivateKey, 0, 128, RndBytes);
			byte[] PubKey = CryptoData.MakePublicKey1024(MyPrivateKey);
			Buffer.BlockCopy(PubKey, 0, DataToSend, 0, 128);


			byte[] PvtPartForSharedKey=new byte[128];
			ParanoidRNG.GetBytes(PvtPartForSharedKey,0,128,RndBytes);
			PubKey = CryptoData.MakePublicKey1024(PvtPartForSharedKey);
			Buffer.BlockCopy(PubKey, 0, DataToSend, 128, 128);

			SharedKey = CryptoData.MakeSharedKey1024(PvtPartForSharedKey, SharedKey);


			StorageKey = new byte[128];
			MsgIdTranslationBytes = new byte[16];

			ParanoidRNG.GetBytes(StorageKey, 0, 128, RndBytes);
			ParanoidRNG.GetBytes(MsgIdTranslationBytes, 0, 16, RndBytes);


			Message.PostMessage(ParentAccount.AccountID, 0, ContactID, ParentAccount.AccountID, (int)MsgType.AddContactAccepted, ParanoidHelpers.MakeEncryptedMessage(SendingReplyKey, DataToSend));

			for (int i = 0; i < 128; i++)
			{
				SendingReplyKey[i] = 0;
				PubKey[i] = 0;
				PvtPartForSharedKey[i] = 0;
			}
			for (int i = 0; i < 256; i++) DataToSend[i] = 0;

			StatusOfContact = ContactStatus.Estabilished;
			CryptoData.SaveKeys();
			BackgroundTasks.SendReceiveAccount(ParentAccount);
		}

		public static bool ProcessIncomingContactAccept(Account Acc, Message Msg)
		{
			Contact Cnt = Acc.FindContact(Msg.FromUser, Msg.FromServer);
			if (Cnt?.Status != ContactStatus.RequestSent) return false;

			byte[] tmp = CryptoData.MakeSharedKey1024(Cnt.MyPrivateKey,Cnt.ExtraData);
			if (tmp == null) return false;
			byte[] DecryptedMessage = ParanoidHelpers.DecryptMessage(tmp, Msg.MessageBody);

			if (DecryptedMessage?.Length != 256) return false;

			Cnt.OtherSidePublicKey=new byte[128];
			Buffer.BlockCopy(DecryptedMessage,0, Cnt.OtherSidePublicKey, 0,128);

			Buffer.BlockCopy(DecryptedMessage, 128, tmp, 0, 128);
			Cnt.SharedKey = CryptoData.MakeSharedKey1024(Cnt.SharedKey, tmp);

			for (int i = 0; i < 128; i++) tmp[i] = 0;
			for (int i = 0; i < DecryptedMessage.Length; i++) DecryptedMessage[i] = 0;
			Cnt.ExtraData = null;
			Cnt.StatusOfContact = ContactStatus.Estabilished;
			CryptoData.SaveKeys();
			return true;
		}

		public void DeleteContact(bool isFullyRemove)
		{
			if (isFullyRemove)
			{
				using (DB DBC=new DB())
				{

					DBC.Conn.Execute(
						"Delete from Messages where FromServer=0 and FromUser=@AccID and ToServer=@AccID and ToUser=@CntID",
						new {AccID = ParentAccount.AccountID, CntID = ContactID});
					DBC.Conn.Execute("Delete from Messages where FromServer=@AccID and FromUser=@CntID and ToServer=0 and ToUser=@AccID",
						new { AccID = ParentAccount.AccountID, CntID = ContactID });
				}
				ParentAccount.Contacts.Remove(this);
			}
			else
			{
				StatusOfContact=ContactStatus.Blocked;
			}
			CryptoData.SaveKeys();
		}

		public long TranslateMsgID(long MsgID)
		{
			long[] Result = new long[4];
			HashLib.Crypto.SHA3.Blake256 Hash = new HashLib.Crypto.SHA3.Blake256();
			Hash.TransformLong(MsgID);
			Hash.TransformBytes(MsgIdTranslationBytes);
			byte[] tmp = (Hash.TransformFinal()).GetBytes();
			Buffer.BlockCopy(tmp, 0, Result, 0, tmp.Length);
			Result[0] = Result[0] ^ Result[1] ^ Result[2] ^ Result[3];

			return Result[0];
		}

		public byte[] RecodeReceivedMsg(byte[] MessageBody)
		{
			if (MessageBody.Length < 416) return null;
			byte[] MessageKey=new byte[128];
			Buffer.BlockCopy(MessageBody,0,MessageKey,0,128);
			//
			MessageKey = CryptoData.MakeSharedKey1024(MyPrivateKey, MessageKey);
			for (int i = 0; i < 128; i++)
				MessageKey[i] ^= SharedKey[i];

			byte[] Data=new byte[MessageBody.Length-128];
			Buffer.BlockCopy(MessageBody,128,Data,0,MessageBody.Length-128);

			byte[] DecodedMsg = ParanoidHelpers.DecryptMessage(MessageKey, Data);
			if (DecodedMsg == null) return null;

			Data = ParanoidHelpers.MakeEncryptedMessage(StorageKey, DecodedMsg);
			for (int i = 0; i < DecodedMsg.Length; i++) DecodedMsg[i] = 0;

			return Data;
		}

		public byte[] EncodeMsgForSending(byte[] MessageBody)
		{
			byte[] DecodedMsg = ParanoidHelpers.DecryptMessage(StorageKey, MessageBody);
			if (DecodedMsg == null) return null;

			byte[] TempPvtKey=new byte[128];
			ParanoidRNG.GetBytes(TempPvtKey,0,128);
			byte[] TempPubKey = CryptoData.MakePublicKey1024(TempPvtKey);

			byte[] TempSharedKey = CryptoData.MakeSharedKey1024(TempPvtKey, OtherSidePublicKey);
			for (int i = 0; i < 128; i++) TempSharedKey[i] ^= SharedKey[i];
			byte[] EncodedMsgBody = ParanoidHelpers.MakeEncryptedMessage(TempSharedKey, DecodedMsg);
			for (int i = 0; i < 128; i++)
			{
				TempPvtKey[i] = 0;
				TempSharedKey[i] = 0;
			}
			for (int i = 0; i < DecodedMsg.Length; i++) DecodedMsg[i] = 0;

			byte[] RetValue=new byte[128+EncodedMsgBody.Length];
			Buffer.BlockCopy(TempPubKey,0,RetValue,0,128);
			Buffer.BlockCopy(EncodedMsgBody,0,RetValue,128, EncodedMsgBody.Length);
			return RetValue;
		}

		public static bool ProcessUpdateUserAuthKey(Account Acc, Message Msg)
		{
			Contact Cnt = Acc.FindContact(Msg.FromUser, Msg.FromServer);
			if ((Cnt.Status != ContactStatus.Estabilished)||(Msg.MessageBody.Length != 96)) return false;

			if (!Ed25519.Verify(new ArraySegment<byte>(Msg.MessageBody,32,64),new ArraySegment<byte>(Msg.MessageBody,0,32),new ArraySegment<byte>(Cnt.RemoteAuthPublicKey,0,32))) return false;

			Buffer.BlockCopy(Msg.MessageBody,0,Cnt.RemoteAuthPublicKey,0,32);

			return true;
		}

	}
}
