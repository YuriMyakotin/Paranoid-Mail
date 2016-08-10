using System;
using System.Text;
using HashLib.Crypto.SHA3;

namespace Paranoid
{
    public static class Pwd2Key
    {
        public static byte[] PasswordToKey(string Pwd,int Rounds, int N)
        {
            if ((Rounds<=0)||(N<=0)) throw new Exception("Invalid values");

            Blake512 Bl512=new Blake512();

            SkeinFish.Skein1024 SkeinBig=new SkeinFish.Skein1024();
            SkeinBig.Initialize();
            byte[] KeyHash=SkeinBig.ComputeHash(Encoding.UTF32.GetBytes(Pwd));
            SkeinBig.Initialize();


            Keccak384 K=new Keccak384();
            K.Initialize();
            byte[] buf=K.ComputeBytes(Encoding.UTF8.GetBytes(Pwd)).GetBytes();
            K.Initialize();

            ChaCha Ch20 = new ChaCha(20);
            Ch20.Init(buf,0,buf,32,buf,40);

            buf=new byte[256*Rounds];

            for (int i = 0; i < Rounds; i++) //prepare
            {
                Ch20.Get512BitsBlock(buf, 256*i);
                Ch20.Get512BitsBlock(buf, 192+256 * i);
                Ch20.Get512BitsBlock(buf, 128+256 * i);
                Ch20.Get512BitsBlock(buf, 64+256 * i);

            }





            for (int i = 0; i < Rounds; i++)
            {
                Bl512.TransformBytes(KeyHash);

                for (int j = 0; j < N; j++)
                {
                    byte[] tempbytes = new byte[4];
                    Ch20.EncryptBytes(tempbytes,0,4);
                    Bl512.TransformBytes(buf,(int)((BitConverter.ToUInt32(tempbytes, 0) * 256 * Rounds - 47) / uint.MaxValue),47);
                }
                Bl512.TransformBytes(KeyHash);
                byte[] tmp = (Bl512.TransformFinal()).GetBytes();
                Bl512.Initialize();

                byte[] tempbytes2 = new byte[2];
                Ch20.EncryptBytes(tempbytes2, 0, 2);
                Buffer.BlockCopy(tmp,0,buf, 256*i+(192*(BitConverter.ToUInt16(tempbytes2,0))/ushort.MaxValue),64);


            }



            byte[] RetVal = SkeinBig.ComputeHash(buf);
            SkeinBig.Initialize();

            Ch20.EncryptBytes(RetVal,0,RetVal.Length);


            for (int i = 0; i < buf.Length; i++) buf[i] = 0;
            for (int i = 0; i < KeyHash.Length; i++) KeyHash[i] = 0;
            Ch20.Clear();

            return RetVal;
        }
    }
}
