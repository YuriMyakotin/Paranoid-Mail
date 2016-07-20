

using System;
using System.Data;

namespace Paranoid
{
    public class StreamCipher
    {
        protected byte[] Block;
        protected int UsedBytes = -1;

        public virtual int KeyDataSize => 0;

        public virtual void Init(byte[] KeyData,int Offset=0) {throw new NotImplementedException();}

        protected virtual void NextBlock() { throw new NotImplementedException(); }

        public void EncryptBytes(byte[] Data, int Offset, int Len)
        {
            if (UsedBytes < 0) NextBlock(); //first call

            for (var i = 0; i < Len; i++)
            {
                Data[i + Offset] ^= Block[UsedBytes];
                ++UsedBytes;
                if (UsedBytes == Block.Length) NextBlock();
            }
        }

        public virtual void Clear()
        {
            for (var i = 0; i < Block.Length; i++)
                Block[i] = 0;
        }

    }
}
