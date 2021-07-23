using System;

namespace Suspension.Samples
{
    public sealed partial class Arithmetic
    {
        [Suspendable]
        public static void IntPlus(int a, int b, Action<int> report)
        {
            report(a + b);
            report(a + 7);
            //report(-13 + a);
            //a = a + 10;
            //report(a);
            //var c = a + 19;
            //report(c);
            //b += 9;
            //report(b);
            //report(a + b);
        }
    }
}