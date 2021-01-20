using System;

namespace Suspension.Tests.Samples.SimpleSuspensionPoint
{
    public static partial class Class
    {
        [Suspendable]
        public static void Execute(Action<string> action)
        {
            action("Hello");
            Flow.Suspend("Middle");
            action("World");
        }
    }
}