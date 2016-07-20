using System;
using SkeinFish;
using HashLib.Crypto.SHA3;

namespace Paranoid
{
	class ThreeFish256Stream:StreamCipher
	{
		private readonly Threefish256 TF;
		private readonly ulong[] Ctr=new ulong[4];
		readonly ulong[] ULData = new ulong[4];

		public ThreeFish256Stream()
		{
			TF=new Threefish256();
			Block=new byte[32];
		}

		public override int KeyDataSize =>80 ;

		public override void Init(byte[] KeyData, int DataOffset=0)
		{
			int Lenght = KeyData.Length / 2;
			if (KeyData.Length - DataOffset < Lenght) throw new Exception("Invalid Data Lenght");
			Blake256 Hash = new Blake256();
			Hash.TransformBytes(KeyData, DataOffset, Lenght-48);
			byte[] HashBytes = (Hash.TransformFinal()).GetBytes();
			Hash.Initialize();

			ulong[] Key=new ulong[4];
			ulong[] Tweak=new ulong[2];
			Buffer.BlockCopy(HashBytes,0,Key,0,32);

			Buffer.BlockCopy(KeyData, DataOffset + Lenght-48, Ctr, 0, 32);
			Buffer.BlockCopy(KeyData, DataOffset+Lenght-16, Tweak, 0, 16);

			TF.SetKey(Key);
			TF.SetTweak(Tweak);

			for (int i = 0; i < Key.Length; i++) Key[i] = 0;
			for (int i = 0; i < HashBytes.Length; i++) HashBytes[i] = 0;

			Tweak[0] = 0;
			Tweak[1] = 0;

		}

		protected override void NextBlock()
		{
			TF.Encrypt(Ctr,ULData);
			Buffer.BlockCopy(ULData,0,Block,0,32);
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
