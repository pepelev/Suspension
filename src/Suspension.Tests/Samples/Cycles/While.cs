using System;

namespace Suspension.Tests.Samples.Cycles
{
    public class While
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            while (argument())
            {
                Flow.Suspend("InsideWhile");
                action("Hello");
            }

            Flow.Suspend("OutsideWhile");
            action("World");
        }
    }
}