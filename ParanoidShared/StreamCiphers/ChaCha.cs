using System;
using HashLib.Crypto.SHA3;
using SkeinFish;

namespace Paranoid
{
	//-----------------------------------------------------------------------------------------------------
	public class ChaCha:StreamCipher
	{
		private readonly byte rounds;
		private readonly uint[] state;
		//private readonly byte[] Block;
	   // private int UsedBytes;

		public override int KeyDataSize => 48;
		//-----------------------------------------------------------------------------------------------------
		public ChaCha( byte rnds)
		{

			state = new uint[16];
			rounds = rnds;
			state[0] = 1634760805;
			state[1] = 857760878;
			state[2] = 2036477234;
			state[3] = 1797285236;

			Block = new byte[64];
		}

		public void Init(byte[] key, int KeyOffset, byte[] iv, int IVOffset, byte[] Ctr, int CtrOffset)
		{
			Buffer.BlockCopy(key, KeyOffset, state, 16, 32);

			Buffer.BlockCopy(iv, IVOffset, state, 56, 8);
			Buffer.BlockCopy(Ctr, CtrOffset, state, 48, 8);
			//UsedBytes = -1;
		}

		public override void Init(byte[] KeyData,int DataOffset=0)
		{
			int Lenght = KeyData.Length / 2;
			if (KeyData.Length-DataOffset<Lenght) throw new Exception("Invalid Data Lenght");
			Skein384 SK = new Skein384();
            SK.Initialize();
            //
            byte[] ChaChaKey=SK.ComputeHash(KeyData, DataOffset, Lenght);

			//
			SK.Dispose();

			Init(ChaChaKey,0, ChaChaKey, 32, ChaChaKey, 40);
			for (int i = 0; i < ChaChaKey.Length; i++)
				ChaChaKey[i] = 0;
		}


		//-----------------------------------------------------------------------------------------------------
		public void SetCounter(byte[] CtrBytes, int CtrOffset)
		{
			if ((CtrBytes.Length - CtrOffset) < 8)
			{
				throw new ArgumentException("Invalid size");
			}
			Buffer.BlockCopy(CtrBytes, CtrOffset, state, 48, 8);
			UsedBytes = -1;
		}

		//-----------------------------------------------------------------------------------------------------
		private void DoubleRounds()
		{
			unchecked
			{
				uint[] x = new uint[16];
				int i;
				Buffer.BlockCopy(state, 0, x, 0, 64);

				for (i = 0; i < rounds/2; i++)
				{
					x[0] = x[0] + x[4];
					x[12] = (x[12] ^ x[0]) << 16 | (x[12] ^ x[0]) >> 16;
					x[8] = x[8] + x[12];
					x[4] = (x[4] ^ x[8]) << 12 | (x[4] ^ x[8]) >> 20;
					x[0] = x[0] + x[4];
					x[12] = (x[12] ^ x[0]) << 8 | (x[12] ^ x[0]) >> 24;
					x[8] = x[8] + x[12];
					x[4] = (x[4] ^ x[8]) << 7 | (x[4] ^ x[8]) >> 25;
					x[1] = x[1] + x[5];
					x[13] = (x[13] ^ x[1]) << 16 | (x[13] ^ x[1]) >> 16;
					x[9] = x[9] + x[13];
					x[5] = (x[5] ^ x[9]) << 12 | (x[5] ^ x[9]) >> 20;
					x[1] = x[1] + x[5];
					x[13] = (x[13] ^ x[1]) << 8 | (x[13] ^ x[1]) >> 24;
					x[9] = x[9] + x[13];
					x[5] = (x[5] ^ x[9]) << 7 | (x[5] ^ x[9]) >> 25;
					x[2] = x[2] + x[6];
					x[14] = (x[14] ^ x[2]) << 16 | (x[14] ^ x[2]) >> 16;
					x[10] = x[10] + x[14];
					x[6] = (x[6] ^ x[10]) << 12 | (x[6] ^ x[10]) >> 20;
					x[2] = x[2] + x[6];
					x[14] = (x[14] ^ x[2]) << 8 | (x[14] ^ x[2]) >> 24;
					x[10] = x[10] + x[14];
					x[6] = (x[6] ^ x[10]) << 7 | (x[6] ^ x[10]) >> 25;
					x[3] = x[3] + x[7];
					x[15] = (x[15] ^ x[3]) << 16 | (x[15] ^ x[3]) >> 16;
					x[11] = x[11] + x[15];
					x[7] = (x[7] ^ x[11]) << 12 | (x[7] ^ x[11]) >> 20;
					x[3] = x[3] + x[7];
					x[15] = (x[15] ^ x[3]) << 8 | (x[15] ^ x[3]) >> 24;
					x[11] = x[11] + x[15];
					x[7] = (x[7] ^ x[11]) << 7 | (x[7] ^ x[11]) >> 25;

					x[0] = x[0] + x[5];
					x[15] = (x[15] ^ x[0]) << 16 | (x[15] ^ x[0]) >> 16;
					x[10] = x[10] + x[15];
					x[5] = (x[5] ^ x[10]) << 12 | (x[5] ^ x[10]) >> 20;
					x[0] = x[0] + x[5];
					x[15] = (x[15] ^ x[0]) << 8 | (x[15] ^ x[0]) >> 24;
					x[10] = x[10] + x[15];
					x[5] = (x[5] ^ x[10]) << 7 | (x[5] ^ x[10]) >> 25;
					x[1] = x[1] + x[6];
					x[12] = (x[12] ^ x[1]) << 16 | (x[12] ^ x[1]) >> 16;
					x[11] = x[11] + x[12];
					x[6] = (x[6] ^ x[11]) << 12 | (x[6] ^ x[11]) >> 20;
					x[1] = x[1] + x[6];
					x[12] = (x[12] ^ x[1]) << 8 | (x[12] ^ x[1]) >> 24;
					x[11] = x[11] + x[12];
					x[6] = (x[6] ^ x[11]) << 7 | (x[6] ^ x[11]) >> 25;
					x[2] = x[2] + x[7];
					x[13] = (x[13] ^ x[2]) << 16 | (x[13] ^ x[2]) >> 16;
					x[8] = x[8] + x[13];
					x[7] = (x[7] ^ x[8]) << 12 | (x[7] ^ x[8]) >> 20;
					x[2] = x[2] + x[7];
					x[13] = (x[13] ^ x[2]) << 8 | (x[13] ^ x[2]) >> 24;
					x[8] = x[8] + x[13];
					x[7] = (x[7] ^ x[8]) << 7 | (x[7] ^ x[8]) >> 25;
					x[3] = x[3] + x[4];
					x[14] = (x[14] ^ x[3]) << 16 | (x[14] ^ x[3]) >> 16;
					x[9] = x[9] + x[14];
					x[4] = (x[4] ^ x[9]) << 12 | (x[4] ^ x[9]) >> 20;
					x[3] = x[3] + x[4];
					x[14] = (x[14] ^ x[3]) << 8 | (x[14] ^ x[3]) >> 24;
					x[9] = x[9] + x[14];
					x[4] = (x[4] ^ x[9]) << 7 | (x[4] ^ x[9]) >> 25;
				}

				for (i = 0; i < 16; ++i)
				{
					x[i] += state[i];
				}
				Buffer.BlockCopy(x, 0, Block, 0, 64);
			}
		}

		//-----------------------------------------------------------------------------------------------------
		protected override void NextBlock()
		{
			DoubleRounds();
			++state[12];
			if (state[12] == 0)
			{
				++state[13];
			}
			UsedBytes = 0;
		}

		//-----------------------------------------------------------------------------------------------------

		public void Get512BitsBlock(byte[] Data, int Offset)
		{
			NextBlock();
			Buffer.BlockCopy(Block, 0, Data, Offset, 64);
		}

		public override void Clear()
		{
			for (var i = 0; i < 16; i++)
			{
				state[i] = 0;
			}
			base.Clear();
		}
	}
}