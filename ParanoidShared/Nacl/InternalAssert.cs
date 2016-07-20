using System;

namespace Chaos.NaCl.Internal
{
    public static class InternalAssert
    {
        public static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("An assertion in Chaos.Crypto failed " + message);
        }
    }
}
