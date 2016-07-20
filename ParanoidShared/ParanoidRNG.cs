using System;
using System.Threading;
using System.Security.Cryptography;
using HashLib.Crypto.SHA3;



namespace Paranoid
{
	public static class ParanoidRNG
	{
		private static int LastCallTime;
		private static long CallsCtr;
		private static readonly long CallsIncrement;

		static ParanoidRNG()
		{
			LastCallTime = Environment.TickCount;
			byte[] tmp = new byte[8];
			using (RNGCryptoServiceProvider CRNG = new RNGCryptoServiceProvider())
			{
				CRNG.GetBytes(tmp);
				CallsCtr = BitConverter.ToInt64(tmp, 0);
				CRNG.GetBytes(tmp);
				CallsIncrement = BitConverter.ToInt64(tmp, 0);
			}

		}

		public static void GetBytes(byte[] Buff, int BuffOffset=0, int BytesLen=0,byte[] ExtraData=null)
		{
			if (BytesLen == 0) BytesLen = Buff.Length - BuffOffset;
			if ((Buff.Length < BuffOffset + BytesLen)||(BuffOffset<0)||(BytesLen<=0)) throw new ArgumentException("Invalid Array size");
			Skein384 SK=new Skein384();

			using (RNGCryptoServiceProvider CRNG = new RNGCryptoServiceProvider())
			{
				byte[] Rnd1 = new byte[BytesLen];

				CRNG.GetBytes(Rnd1);


				int Ticks = Environment.TickCount;
				int OldTicks = Interlocked.Exchange(ref LastCallTime, Ticks);
				long Ctr = Interlocked.Add(ref CallsCtr, CallsIncrement);


				SK.TransformInt(Ticks);
				SK.TransformLong(Environment.WorkingSet);
				SK.TransformLong(DateTime.Now.Ticks);
				SK.TransformInt(Ticks - OldTicks);
				SK.TransformInt(Thread.CurrentThread.ManagedThreadId);

				byte[] Rnd_tmp = new byte[19];
				CRNG.GetBytes(Rnd_tmp);
				SK.TransformBytes(Rnd_tmp);
				SK.TransformLong(Ctr);


				if (ExtraData != null) SK.TransformBytes(ExtraData);

				byte[] Hash = (SK.TransformFinal()).GetBytes();
				SK.Initialize();

				ChaCha Ch20 = new ChaCha(20);
				Ch20.Init(Hash, 0, Hash, 32, Hash, 40);
				Ch20.EncryptBytes(Rnd1, 0, BytesLen);
				Ch20.Clear();

				for (int i = 0; i < Hash.Length; i++) Hash[i] = 0;
				for (int i = 0; i < Rnd_tmp.Length; i++) Rnd_tmp[i] = 0;

				Buffer.BlockCopy(Rnd1, 0, Buff, BuffOffset, BytesLen);
			}

		}
	}
}
