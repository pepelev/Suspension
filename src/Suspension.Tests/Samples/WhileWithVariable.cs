using System;
using System.Globalization;

namespace Suspension.Tests.Samples
{
    public partial class WhileWithVariable
    {
        //[Suspendable] todo uncomment
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            while (argument())
            {
                var d = 10;
                Flow.Suspend("InsideWhile");
                action("Hello");
                while (argument())
                {
                    d += 2;
                    action("visited check " + d.ToString(CultureInfo.InvariantCulture));
                }
            }

            Flow.Suspend("OutsideWhile");
            action("World");
        }
    }
}