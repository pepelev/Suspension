using System;

namespace Suspension.Playground
{
    public partial class SimpleWhile
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

            Flow.Suspend("OutsideWhile");
            action("World");
        }
    }
}