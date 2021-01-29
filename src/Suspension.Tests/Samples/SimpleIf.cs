using System;

namespace Suspension.Tests.Samples
{
    public partial class SimpleIf
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