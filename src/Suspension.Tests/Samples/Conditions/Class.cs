using System;

namespace Suspension.Tests.Samples.Conditions
{
    public static partial class Class
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            if (argument())
            {
                Flow.Suspend("InsideIf");
                action("Hello");
            }

            Flow.Suspend("OutsideIf");
            action("World");
        }
    }
}
