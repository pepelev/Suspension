using System;

namespace Suspension.Samples
{
    public sealed partial class Syntax
    {
        [Suspendable]
        public static void IntPlus(int a, int b, Action<int> report)
        {
            report(-13 + a);
        }

#if Literal
        [Suspendable]
        public static void LongLiteral()
        {
#if Literal_Int
            var @int = -42;
#endif
#if Literal_UInt
            var @uint = 93U;
#endif
#if Literal_Long
            var @long = 10L;
#endif
#if Literal_ULong
            var @ulong = 10UL;
#endif
#if Literal_String
            var @string = "hello";
#endif
#if Literal_Char
            var @char = 'a';
#endif
#if Literal_Float
            var @float = 12.9f;
#endif
#if Literal_Double
            var @double = 12.9;
#endif
#if Literal_Decimal
            var @double = 12.9m;
#endif
        }
#endif
    }
}