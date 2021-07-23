using System;

namespace Suspension.Tests.Samples
{
    public partial class While
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            while (argument())
            {
                Flow.Suspend("InsideWhile");
                action("Hello");
                while (argument())
                {
                    action("visited check");
                }
            }

            action("before outside");
            Flow.Suspend("OutsideWhile");
            action("World");
        }
    }
}