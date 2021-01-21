using System;

namespace Suspension.Tests.Samples.Conditions
{
    public static partial class Class
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        { // 0
            if (argument())
            {
                Flow.Suspend("InsideIf");
                action("Hello"); // 2
            }

            Flow.Suspend("OutsideIf");
            action("World"); // 3
        } // 4
    }
}
