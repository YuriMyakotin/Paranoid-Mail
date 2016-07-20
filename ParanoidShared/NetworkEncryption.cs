using System;
using System.Data;
using Chaos.NaCl;
using HashLib.Crypto.SHA3;

namespace Paranoid
{
	public enum CryptoProtocols : ulong //should be 2^n
	{
		ChaCha20=1,
		ThreeFish256=2,
		ThreeFish512=4
	}

	public class NetworkEncryption
	{
		public StreamCipher Encoder;
		public StreamCipher Decoder;
		private byte[] SecretKey;
		private readonly Skein512 Hash;
		public const int KeyBlockSize = 1024;

		public NetworkEncryption()
		{
			Hash = new Skein512();
		}

		public void AddHashData(byte[] Data) => Hash.TransformBytes(Data);

		public void Init(ulong SelectedSendingCipher, ulong SelectedReceivingCipher)
		{
			switch (SelectedSendingCipher)
			{
				case (ulong)CryptoProtocols.ChaCha20:
					Encoder=new ChaCha(20);
					break;

				case (ulong)CryptoProtocols.ThreeFish256:
					Encoder = new ThreeFish256Stream();
					break;
				case (ulong)CryptoProtocols.ThreeFish512:
					Encoder = new ThreeFish512Stream();
					break;

				default: throw new ArgumentException("Not supported");
			}

			switch (SelectedReceivingCipher)
			{
				case (ulong)CryptoProtocols.ChaCha20:
					Decoder = new ChaCha(20);
					break;

				case (ulong)CryptoProtocols.ThreeFish256:
					Decoder = new ThreeFish256Stream();
					break;
				case (ulong)CryptoProtocols.ThreeFish512:
					Decoder = new ThreeFish512Stream();
					break;

				default: throw new ArgumentException("Not supported");
			}

			SecretKey = new byte[KeyBlockSize];
			ParanoidRNG.GetBytes(SecretKey);
		}

		public byte[] MakePublicKeysBlock()
		{
			byte[] Result = new byte[KeyBlockSize];
			for (int i = 0; i < KeyBlockSize / 32; i++)
			{
				ArraySegment<byte> SKeyPart = new ArraySegment<byte>(SecretKey, i * 32, 32);
				ArraySegment<byte> PKeyPart = new ArraySegment<byte>(Result, i * 32, 32);
				MontgomeryCurve25519.GetPublicKey(PKeyPart, SKeyPart);

			}
			return Result;
		}

		public void SetSharedKey(byte[] RemotePublicKey, bool isCaller)
		{
			if (RemotePublicKey.Length!= KeyBlockSize) throw new ArgumentException();

			byte[] SharedKey = new byte[KeyBlockSize];

			for (int i = 0; i < KeyBlockSize / 32; i++)
			{
				var Tmp1 = new ArraySegment<byte>(SecretKey, i * 32, 32);
				var Tmp2 = new ArraySegment<byte>(RemotePublicKey, i * 32, 32);
				var Tmp3 = new ArraySegment<byte>(SharedKey, i * 32, 32);
				MontgomeryCurve25519.KeyExchange(Tmp3, Tmp2, Tmp1);
			}


			if (isCaller)
			{
				Encoder.Init(SharedKey, 0);
				Decoder.Init(SharedKey, KeyBlockSize / 2);
			}
			else
			{
				Decoder.Init(SharedKey, 0);
				Encoder.Init(SharedKey, KeyBlockSize / 2);
			}

			for (int i = 0; i < KeyBlockSize; i++)
			{
				SecretKey[i] = 0;
				SharedKey[i] = 0;
			}


			SecretKey = null;

		}


		public byte[] MakeSignature(byte[] ServerPrivateKey) => Ed25519.Sign((Hash.TransformFinal()).GetBytes(), Ed25519.ExpandedPrivateKeyFromSeed(ServerPrivateKey));

		public bool VerifySignature(byte[] Signature, byte[] ServerPublicKey) => Ed25519.Verify(Signature, (Hash.TransformFinal()).GetBytes(), ServerPublicKey);



		public void Clear()
		{
			Hash.Initialize();
			Encoder?.Clear();
			Decoder?.Clear();
		}

	}
}
