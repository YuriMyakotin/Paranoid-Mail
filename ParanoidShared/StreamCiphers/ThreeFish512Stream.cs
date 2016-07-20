using System;
using SkeinFish;
using HashLib.Crypto.SHA3;

namespace Paranoid
{
	class ThreeFish512Stream : StreamCipher
	{
		private readonly Threefish512 TF;
		private readonly ulong[] Ctr = new ulong[8];
		private readonly ulong[] ULData = new ulong[8];

		public ThreeFish512Stream()
		{
			TF = new Threefish512();
			Block = new byte[64];
		}

		public override int KeyDataSize => 144;

		public override void Init(byte[] KeyData,int DataOffset= 0)
		{
			int Lenght = KeyData.Length / 2;
			if (KeyData.Length - DataOffset < Lenght) throw new Exception("Invalid Data Lenght");
			Blake512 Hash = new Blake512();
			Hash.TransformBytes(KeyData, DataOffset, Lenght - 80);
			byte[] HashBytes = (Hash.TransformFinal()).GetBytes();
			Hash.Initialize();


			ulong[] Key = new ulong[8];
			ulong[] Tweak = new ulong[2];
			Buffer.BlockCopy(HashBytes, 0, Key, 0, 64);

			Buffer.BlockCopy(KeyData, DataOffset + Lenght - 80, Ctr, 0, 64);
			Buffer.BlockCopy(KeyData, DataOffset + Lenght - 16, Tweak, 0, 16);

			TF.SetKey(Key);
			TF.SetTweak(Tweak);
			for (int i = 0; i < Key.Length; i++) Key[i] = 0;
			for (int i = 0; i < HashBytes.Length; i++) HashBytes[i] = 0;

			Tweak[0] = 0;
			Tweak[1] = 0;

		}

		protected override void NextBlock()
		{
			TF.Encrypt(Ctr, ULData);
			Buffer.BlockCopy(ULData, 0, Block, 0, 64);
			++Ctr[0];
			if (Ctr[0] == 0) ++Ctr[1];
			UsedBytes = 0;
		}

		public override void Clear()
		{
			for (int i = 0; i < ULData.Length; i++) ULData[i] = 0;
			for (int i = 0; i < Ctr.Length; i++) Ctr[i] = 0;
			TF.SetKey(Ctr);
			TF.SetTweak(new ulong[2]);
			base.Clear();
		}
	}
}
