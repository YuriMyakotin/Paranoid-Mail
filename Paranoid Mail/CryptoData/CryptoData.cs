using Chaos.NaCl;
using ProtoBuf;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Paranoid
{
	public enum LoadKeysResult : int
	{
		Ok = 0,
		FileOpeningError = 1,
		InvalidFileSize = 2,
		InvalidData = 3
	}

	public static class CryptoData
	{
		public static ObservableCollection<Account> Accounts;
		private static byte[] MasterKey;
		public static string FileName;

		public static object LockObject=new object();



		private static byte[] Str2Key(string Str) => Pwd2Key.PasswordToKey(Str, 55243, 61);

		public static byte[] MakePublicKey1024(byte[] MyPvtKey)
		{
			if (MyPvtKey?.Length != 128) return null;
			byte[] RetVal = new byte[128];
			;
			for (int i = 0; i < 4; i++)
			{
				ArraySegment<byte> RetValPart = new ArraySegment<byte>(RetVal, i * 32, 32);
				MontgomeryCurve25519.GetPublicKey(RetValPart, new ArraySegment<byte>(MyPvtKey, i * 32, 32));
			}
			return RetVal;
		}

		public static byte[] MakeSharedKey1024(byte[] MyPvtKey, byte[] RemotePubKey)
		{
			if ((MyPvtKey == null) || (RemotePubKey == null)) return null;
			if ((MyPvtKey.Length != 128) || (RemotePubKey.Length != 128)) return null;
			byte[] RetVal = new byte[128];
			for (int i = 0; i < 4; i++)
			{
				ArraySegment<byte> RetValPart = new ArraySegment<byte>(RetVal, i * 32, 32);
				MontgomeryCurve25519.KeyExchange(RetValPart, new ArraySegment<byte>(RemotePubKey, i * 32, 32), new ArraySegment<byte>(MyPvtKey, i * 32, 32));
			}
			return RetVal;
		}

		public static int isPasswordStrong(string Pwd)
		{
			if (Pwd.Length < 12) return 0;

			char[] PwdChars = Pwd.ToCharArray();
			int Cnt = PwdChars.Distinct().Count();
			for(int i=0;i<PwdChars.Length;i++) PwdChars[i] = '\x00';

			if (Cnt < 8) return 1;

			int UpChars = 0, LowChars = 0, Symbols = 0, NonAscii = 0;
			for (int i = 0; i < Pwd.Length; i++)
			{
				int CharCode = char.ConvertToUtf32(Pwd, i);
				if ((CharCode < 65) || ((CharCode > 90) && (CharCode < 97)) || ((CharCode > 122) && (CharCode < 128))) Symbols = 1;
				else if (CharCode <= 90) UpChars = 1;
				else if (CharCode <= 122) LowChars = 1;
				else ++NonAscii;
			}
			if (NonAscii != 0)
			{
				if (NonAscii < 3) NonAscii = 1;
				else if (NonAscii < 10) NonAscii = 3;
			}

			return UpChars + LowChars + Symbols + NonAscii;
		}

		public static bool ComparePassword(string OldPassword)
		{
			if (MasterKey == null) return false;
			byte[] PwdToCompare = Str2Key(OldPassword);

			bool result = Chaos.NaCl.CryptoBytes.ConstantTimeEquals(MasterKey, PwdToCompare);
			for (int i = 0; i < PwdToCompare.Length; i++) PwdToCompare[i] = 0;
			return result;
		}

		public static void SetMasterKey(string KeyStr) => MasterKey = Str2Key(KeyStr);

		public static LoadKeysResult LoadKeys()
		{
			byte[] EncryptedData;
			if (FileName == "::DB")
			{
				EncryptedData=Utils.GetBinaryValue("KeyData");
				if (EncryptedData==null) return LoadKeysResult.FileOpeningError;

			}
			else
			{
				try
				{
					EncryptedData = File.ReadAllBytes(FileName);
				}
				catch
				{
					return LoadKeysResult.FileOpeningError;
				}
			}
			if ((EncryptedData.Length < 288) || (EncryptedData.Length % 128 != 32)) return LoadKeysResult.InvalidFileSize;

			byte[] Buf = ParanoidHelpers.DecryptMessage(MasterKey, EncryptedData);
			if (Buf == null) return LoadKeysResult.InvalidData;

			using (MemoryStream MS = new MemoryStream(Buf))
			{
				try
				{
					Accounts = Serializer.Deserialize<ObservableCollection<Account>>(MS);
				}
				catch
				{
					return LoadKeysResult.InvalidData;
				}
			}

			foreach (Account Acc in Accounts)
				foreach (Contact Cnt in Acc.Contacts)
					Cnt.ParentAccount = Acc;

			for (int i = 0; i < Buf.Length; i++)
			{
				Buf[i] = 0;
			}

			return LoadKeysResult.Ok;
		}

		public static bool SaveKeys()
		{
			lock (LockObject)
			{
				byte[] Buf;
				using (MemoryStream MS = new MemoryStream())
				{
					Serializer.Serialize<ObservableCollection<Account>>(MS, Accounts);
					Buf = MS.ToArray();
				}

				byte[] EncryptedData = ParanoidHelpers.MakeEncryptedMessage(MasterKey, Buf);
				for (int i = 0; i < Buf.Length; i++)
				{
					Buf[i] = 0;
				}

				if (FileName == "::DB")
				{
					Utils.UpdateBinaryValue("KeyData",EncryptedData);
					return true;
				}


				try
				{
					File.WriteAllBytes(FileName, EncryptedData);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		public static unsafe void ClearString(this string s)
		{
			if ((s == null) || (s.Length < 1)) return;
			fixed (char* ptr = s)
			{
				for (int i = 0; i < s.Length; i++)
				{
					ptr[i] = '#';
				}
			}
		}

		public static void Clear()
		{
			if (MasterKey != null) for (int i = 0; i < MasterKey.Length; i++) MasterKey[i] = 0;

			if (Accounts == null) return;
			{
				foreach (Account Acc in Accounts)
				{
					if (Acc.AuthPKey != null) for (int i = 0; i < Acc.AuthPKey.Length; i++) Acc.AuthPKey[i] = 0;
					if (Acc.EncryptionPKey != null) for (int i = 0; i < Acc.EncryptionPKey.Length; i++) Acc.EncryptionPKey[i] = 0;
					if (Acc.Contacts != null)
						foreach (Contact Cnt in Acc.Contacts)
						{
							if (Cnt.StorageKey != null) for (int i = 0; i < Cnt.StorageKey.Length; i++) Cnt.StorageKey[i] = 0;
							if (Cnt.SharedKey != null) for (int i = 0; i < Cnt.SharedKey.Length; i++) Cnt.SharedKey[i] = 0;
							if (Cnt.MyPrivateKey != null) for (int i = 0; i < Cnt.MyPrivateKey.Length; i++) Cnt.MyPrivateKey[i] = 0;
							if (Cnt.OtherSidePublicKey != null) for (int i = 0; i < Cnt.OtherSidePublicKey.Length; i++) Cnt.OtherSidePublicKey[i] = 0;
						}
				}
			}
		}
	}
}