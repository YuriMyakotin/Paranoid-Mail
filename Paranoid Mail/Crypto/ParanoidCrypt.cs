using SkeinFish;
using System;
using System.Security.Cryptography;
using HashLib.Crypto.SHA3;
namespace Paranoid
{
	public class ParanoidCrypt
	{


		private readonly Threefish1024 ThreeF;

		private readonly ChaCha StreamCipherGenerator;

		private readonly byte[] HMACKey;

		public ParanoidCrypt(byte[] key, byte[] salt)
		{


			ulong[] TFKey = new ulong[16];

			if ((key.Length != 128) || (salt.Length != 128)) throw new ArgumentException("Invalid arguments length");

			Blake512 Blake = new Blake512();
			Blake.TransformBytes(key, 0, 48);
			Blake.TransformBytes(salt);
			byte[] tmp = (Blake.TransformFinal()).GetBytes();
			Buffer.BlockCopy(tmp, 0, TFKey, 0, 64);
			Blake.Initialize();

			for (int i = 0; i < tmp.Length; i++)
				tmp[i] = 0;

			Keccak512 K512 = new Keccak512();
			K512.TransformBytes(salt);
			K512.TransformBytes(key, 48, 48);
			tmp = (K512.TransformFinal()).GetBytes();

			Buffer.BlockCopy(tmp, 0, TFKey, 64, 64);
			ThreeF = new Threefish1024();
			ThreeF.SetKey(TFKey);
			for (int i = 0; i > 16; i++)
				TFKey[i] = 0;

			for (int i = 0; i < tmp.Length; i++)
				tmp[i] = 0;

			Keccak384 K384 = new Keccak384();


			K384.TransformBytes(key, 96, 32);
			K384.TransformBytes(salt);
			tmp = (K384.TransformFinal()).GetBytes();
			StreamCipherGenerator = new ChaCha(20);
			StreamCipherGenerator.Init(tmp, 0, tmp, 32, tmp, 40);
			K384.Initialize();


			for (int i = 0; i < tmp.Length; i++)
				tmp[i] = 0;



			K512.Initialize();
			K512.TransformBytes(key);
			K512.TransformBytes(salt);
			HMACKey = (K512.TransformFinal()).GetBytes();
			K512.Initialize();
		}

		private byte[] CalculateHMAC(byte[] Data, int Offset, int Len)
		{
			Blake256 Bl=new Blake256();
			Bl.TransformBytes(HMACKey);
			Bl.TransformBytes(Data,Offset,Len);
			Bl.TransformBytes(HMACKey);
			byte[] result = (Bl.TransformFinal()).GetBytes();
			Bl.Initialize();
			return result;
		}


		public byte[] Encrypt(byte[] In, byte[] Salt)
		{
			ulong[] ThreeFTweak = new ulong[2];

			ulong[] Buf1 = new ulong[16];
			ulong[] Buf2 = new ulong[16];

			int Len = In.Length;
			RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
			byte[] tmp = new byte[1];
			rnd.GetNonZeroBytes(tmp);

			int PadAreaSize = (tmp[0]%8 + 1)*128;

			int PadDataSize = Len%128;
			if ((PadDataSize > 120) && (PadAreaSize == 128)) PadAreaSize = 256;
            else if ((PadDataSize == 0) && (PadAreaSize > 768)) PadAreaSize = 256;


			byte[] PadBuf = new byte[PadAreaSize];
			int PadRandomBytesSize = PadAreaSize - PadDataSize;


			rnd.GetBytes(PadBuf);
			rnd.Dispose();

			int NewLen = Len + PadRandomBytesSize + 128 + 32;

			byte[] Out = new byte[NewLen];
			Buffer.BlockCopy(Salt, 0, Out, 0, 128);


			PadBuf[0] = (byte)(PadRandomBytesSize%256);
			PadBuf[1] = (byte) ((PadBuf[1] & 252) |PadRandomBytesSize/256 );


			if (PadDataSize!=0) Buffer.BlockCopy(In, 0, PadBuf, PadRandomBytesSize, PadDataSize);

			byte[] TweakTmp = new byte[16];

			for (int i = 0; i < PadAreaSize/128; i++)
			{
				StreamCipherGenerator.EncryptBytes(TweakTmp, 0, 16);
				Buffer.BlockCopy(TweakTmp, 0, ThreeFTweak, 0, 16);

				ThreeF.SetTweak(ThreeFTweak);
				Buffer.BlockCopy(PadBuf, i * 128, Buf1, 0, 128);
				ThreeF.Encrypt(Buf1, Buf2);

				Buffer.BlockCopy(Buf2, 0, Out, 128 + i*128, 128);
				StreamCipherGenerator.EncryptBytes(Out, 128 + i * 128, 128);
			}

			if (Len != 0)
			{

				for (int j = 0; j < (Len - PadDataSize)/128; j++)
				{

					StreamCipherGenerator.EncryptBytes(TweakTmp, 0, 16);
					Buffer.BlockCopy(TweakTmp, 0, ThreeFTweak, 0, 16);

					ThreeF.SetTweak(ThreeFTweak);
					Buffer.BlockCopy(In, j * 128 + PadDataSize, Buf1, 0, 128);
					ThreeF.Encrypt(Buf1, Buf2);

					Buffer.BlockCopy(Buf2, 0, Out, 128 + PadAreaSize + j*128, 128);
					StreamCipherGenerator.EncryptBytes(Out, 128 + PadAreaSize + j * 128, 128);
				}
			}
			byte[] HMAC = CalculateHMAC(Out, 128, NewLen - 160);
			Buffer.BlockCopy(HMAC, 0, Out, NewLen - 32, 32);

			for (int i = 0; i < TweakTmp.Length; i++)
			{
				TweakTmp[i] = 0;
			}

			for (int i = 0; i < Buf1.Length; i++)
			{
				Buf1[i] = 0;
				Buf2[i] = 0;
			}
			for (int i = 0; i < PadDataSize; i++) PadBuf[i] = 0;

			ThreeFTweak[0] = 0;
			ThreeFTweak[1] = 0;

			return Out;
		}

		public byte[] Decrypt(byte[] Data)
		{
			if ((Data.Length < 288)||(Data.Length % 128 !=32))  return null;

			ulong[] ThreeFTweak = new ulong[2];

			ulong[] Buf1 = new ulong[16];
			ulong[] Buf2 = new ulong[16];


			byte[] tmpBuf = new byte[128];


			byte[] HMAC = CalculateHMAC(Data, 128, Data.Length - 160);

			if (!Chaos.NaCl.CryptoBytes.ConstantTimeEquals(HMAC, 0, Data, Data.Length - 32, 32))
			{
				return null;
			}

			byte[] TweakTmp = new byte[16];


			Buffer.BlockCopy(Data, 128, tmpBuf, 0, 128);
			StreamCipherGenerator.EncryptBytes(TweakTmp, 0, 16);
			StreamCipherGenerator.EncryptBytes(tmpBuf, 0, 128);
			Buffer.BlockCopy(TweakTmp, 0, ThreeFTweak, 0, 16);
			ThreeF.SetTweak(ThreeFTweak);
			Buffer.BlockCopy(tmpBuf, 0, Buf2, 0, 128);
			ThreeF.Decrypt(Buf2, Buf1);
			Buffer.BlockCopy(Buf1, 0, tmpBuf, 0, 128);

			int PadSize = tmpBuf[0] + (tmpBuf[1] & 3) * 256;

			if ((Data.Length - 160 - PadSize) == 0) return new byte[0];

			int CurrentOffset = 256;

			while (CurrentOffset < PadSize + 128)
			{
				Buffer.BlockCopy(Data, CurrentOffset, tmpBuf, 0, 128);
				StreamCipherGenerator.EncryptBytes(TweakTmp, 0, 16);
				StreamCipherGenerator.EncryptBytes(tmpBuf, 0, 128);
				Buffer.BlockCopy(TweakTmp, 0, ThreeFTweak, 0, 16);
				ThreeF.SetTweak(ThreeFTweak);
				Buffer.BlockCopy(tmpBuf, 0, Buf2, 0, 128);
				ThreeF.Decrypt(Buf2, Buf1);
				Buffer.BlockCopy(Buf1, 0, tmpBuf, 0, 128);
				CurrentOffset += 128;
			}



			byte[] Out = new byte[Data.Length - 160 - PadSize];
			int OutOffset=0;
			if (PadSize%128 != 0)
			{
				OutOffset = 128 - (PadSize%128);
				Buffer.BlockCopy(tmpBuf, PadSize%128, Out, 0, 128 - (PadSize%128));
			}


			while (CurrentOffset<Data.Length-32)
			{
				Buffer.BlockCopy(Data, CurrentOffset, tmpBuf, 0, 128);

				StreamCipherGenerator.EncryptBytes(TweakTmp, 0, 16);

				StreamCipherGenerator.EncryptBytes(tmpBuf, 0,128);

				Buffer.BlockCopy(TweakTmp, 0, ThreeFTweak, 0, 16);

				ThreeF.SetTweak(ThreeFTweak);
				Buffer.BlockCopy(tmpBuf, 0, Buf2, 0, 128);
				ThreeF.Decrypt(Buf2, Buf1);
				Buffer.BlockCopy(Buf1, 0, Out, OutOffset, 128);
				CurrentOffset += 128;
				OutOffset+=128;
			}

			for (int i = 0; i < TweakTmp.Length; i++)
			{
				TweakTmp[i] = 0;
			}
			for (int i = 0; i < tmpBuf.Length; i++)
			{
				tmpBuf[i] = 0;
			}
			for (int i = 0; i < Buf1.Length; i++)
			{
				Buf1[i] = 0;
				Buf2[i] = 0;
			}
			ThreeFTweak[0] = 0;
			ThreeFTweak[1] = 0;
			return Out;
		}

		public void Clear()
		{
			StreamCipherGenerator.Clear();
			ThreeF.SetKey(new ulong[16]);
			ThreeF.SetTweak(new ulong[2]);
			for (int i = 0; i < HMACKey.Length; i++) HMACKey[i] = 0;
		}
	}

	public static class ParanoidHelpers
	{
		public static byte[] MakeEncryptedMessage(byte[] key, byte[] Data)
		{
			byte[] Salt = new byte[128];
			ParanoidRNG.GetBytes(Salt);
			ParanoidCrypt P = new ParanoidCrypt(key, Salt);

			byte[] RetVal = P.Encrypt(Data, Salt);
			P.Clear();
			return RetVal;
		}

		public static byte[] DecryptMessage(byte[] key, byte[] Data)
		{

			byte[] Salt = new byte[128];
			Buffer.BlockCopy(Data, 0, Salt, 0, 128);
			ParanoidCrypt P = new ParanoidCrypt(key, Salt);

			byte[] Decrypted = P.Decrypt(Data);
			P.Clear();

			return Decrypted;
		}
	}
}