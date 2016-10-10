using System.Collections.ObjectModel;
using ProtoBuf;
using System;
using Chaos.NaCl;
using System.Linq;
using System.ComponentModel;
using Dapper;

using System.Windows;

namespace Paranoid
{


	public enum AccountStatus : int
	{
		Normal=0,
		MarkedForKeysUpdate=1,
		MarkedForDeletion=100,
		Inactive=255
	}

	[ProtoContract] public class Account : INotifyPropertyChanged
	{
		[ProtoMember(1)] public long AccountID { get; set; }
		[ProtoMember(2)] public long ServerID { get; set; }
		[ProtoMember(3)] public long UserID { get; set; }
		[ProtoMember(4)] public byte[] AuthPKey { get; set; }
		[ProtoMember(5)] public byte[] EncryptionPKey { get; set; }
		[ProtoMember(6)] public string AccountName { get; set; }
		[ProtoMember(7)] public string OverrideIP { get; set; }
		[ProtoMember(8)] public int OverridePort { get; set; }
		[ProtoMember(9)] public string PrivatePortPassword { get; set; }
		[ProtoMember(10)] public AccountStatus Status { get; set; }
		[ProtoMember(11)] public byte[] ExtraData { get; set; }
		[ProtoMember(12)] public ObservableCollection<Contact> Contacts { get; set; }

		public Account Self => this;

		private NetSessionResult _LastCallStatus { get; set; }
		public NetSessionResult LastCallStatus
		{
			get { return _LastCallStatus; }
			set
			{
				_LastCallStatus = value;
				OnPropertyChanged("LastCallStatus");
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}


		public string AccountAddressStr => ((ulong) UserID).ToString() + "@" + ((ulong) ServerID).ToString();
		public string AccountNameStr
		{
			get
			{
				return AccountName.Length >= 1
					? AccountName
					: AccountAddressStr;
			}
			set
			{
				AccountName = value;
				OnPropertyChanged("AccountNameStr");
			}
		}

		public Account()
		{
			Contacts=new ObservableCollection<Contact>();
		}

		public Account(long ServerID, long UserID, string AccountName, string OverrideIP, int OverridePort, string PrivatePassword,
			byte[] RandomData)
		{
			while (true)
			{
				byte[] tmpbytes = new byte[8];
				ParanoidRNG.GetBytes(tmpbytes);
				AccountID = BitConverter.ToInt64(tmpbytes, 0);
				if (CryptoData.Accounts.Count(p => p.AccountID == AccountID) == 0) break;
			}
			this.ServerID = ServerID;
			this.UserID = UserID;
			this.AccountName = AccountName;
			this.OverrideIP = OverrideIP;
			this.OverridePort = OverridePort;
			PrivatePortPassword = PrivatePassword;
			AuthPKey = new byte[32];
			ParanoidRNG.GetBytes(AuthPKey,0,32,RandomData);
			EncryptionPKey = new byte[128];
			ParanoidRNG.GetBytes(EncryptionPKey, 0, 128, RandomData);
			Contacts = new ObservableCollection<Contact>();
		}

		public User MakeUserInfo() => new User
		{
			UserID = UserID,
			AuthKey = Ed25519.PublicKeyFromSeed(AuthPKey),
			EncryptionKey = CryptoData.MakePublicKey1024(EncryptionPKey)
		};

		public void RequestAccountDeletion()
		{
			Status = AccountStatus.MarkedForDeletion;
			Message.PostMessage(AccountID,0, 0, ServerID, (int)MsgType.DeleteUser,
					new byte[1]);
			BackgroundTasks.SendReceiveAccount(this);
		}

		public void AccountDeletionDone()
		{
			using (DB DBC=new DB())
			{

				DBC.Conn.Execute("Delete from Messages where FromServer=0 and FromUser=@AccID", new {AccID = AccountID});
				DBC.Conn.Execute("Delete from Messages where ToServer=0 and ToUser=@AccID", new { AccID = AccountID });
			}
			Application.Current.Dispatcher.Invoke(() => CryptoData.Accounts.Remove(this));
			CryptoData.SaveKeys();
		}

		public void RequestKeysUpdate(byte[] RandomData)
		{
			ExtraData=new byte[160];
			Status = AccountStatus.MarkedForKeysUpdate;

			User UserInfo = new User {UserID = this.UserID};
			byte[] tmp=new byte[32];
			ParanoidRNG.GetBytes(tmp, 0, 32, RandomData);
			Buffer.BlockCopy(tmp,0,ExtraData,0,32);
			UserInfo.AuthKey = Ed25519.PublicKeyFromSeed(tmp);
			for (int i = 0; i < 32; i++)
				tmp[i] = 0;

			tmp=new byte[128];

			ParanoidRNG.GetBytes(tmp, 0, 128, RandomData);
			Buffer.BlockCopy(tmp, 0, ExtraData, 32, 128);
			UserInfo.EncryptionKey = CryptoData.MakePublicKey1024(tmp);
			for (int i = 0; i < 128; i++)
				tmp[i] = 0;

			CryptoData.SaveKeys();

			Message.PostMessage(AccountID, 0, 0, ServerID, (int) MsgType.UpdateUserKeys,
					Utils.ObjectToBytes(UserInfo));
			BackgroundTasks.SendReceiveAccount(this);
		}

		public void KeysUpdateDone()
		{
			if (ExtraData.Length != 160) return;
			Status = AccountStatus.Normal;

			byte[] OldPvtExpandedKey = Ed25519.ExpandedPrivateKeyFromSeed(AuthPKey);
			Buffer.BlockCopy(ExtraData,0,AuthPKey,0,32);
			Buffer.BlockCopy(ExtraData, 32, EncryptionPKey, 0, 128);
			for (int i = 0; i < 160; i++) ExtraData[i] = 0;
			ExtraData = null;
			byte[] NewPubAuthKey= Ed25519.PublicKeyFromSeed(AuthPKey);
			byte[] MsgBody=new byte[96];
			byte[] Sig = Ed25519.Sign(NewPubAuthKey, OldPvtExpandedKey);

			Buffer.BlockCopy(NewPubAuthKey,0,MsgBody,0,32);
			Buffer.BlockCopy(Sig, 0, MsgBody,32,64);

			for (int i = 0; i < OldPvtExpandedKey.Length; i++) OldPvtExpandedKey[i] = 0;

			foreach (Contact Cnt in Contacts)
			{
				if (Cnt.Status == ContactStatus.Estabilished)
					Message.PostMessage(AccountID, 0, Cnt.ContactID, AccountID, (int) MsgType.UpdateUserAuthKey, MsgBody);
			}


			CryptoData.SaveKeys();
		}

		public long MakeNewContactID()
		{
			while (true)
			{
				byte[] tmpbytes = new byte[8];
				ParanoidRNG.GetBytes(tmpbytes);
				long ContactID = BitConverter.ToInt64(tmpbytes, 0);
				if (Contacts.Count(p => p.ContactID == ContactID) == 0) return ContactID;
			}
		}


		public Contact FindContact(long UsrID, long SrvID) => Contacts.SingleOrDefault(
					p => (p.UserID == UsrID) && (p.ServerID == SrvID));

	}

}
