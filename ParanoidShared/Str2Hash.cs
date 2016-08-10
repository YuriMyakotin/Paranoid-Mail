using System;

namespace Paranoid
{
    public static class Str2Hash
    {
        public static ulong StringToHash(string Str)
        {
            long[] Result=new long[4];
            HashLib.Crypto.SHA3.Blake256 Hash = new HashLib.Crypto.SHA3.Blake256();
            HashLib.HashResult HR = Hash.ComputeString(Str);
            byte[] tmp = HR.GetBytes();
            Buffer.BlockCopy(tmp, 0, Result, 0, tmp.Length);
            Result[0] = Result[0] ^ Result[1] ^ Result[2] ^ Result[3];

            return (ulong)Result[0];
        }
    }
}
