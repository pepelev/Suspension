using System;

namespace Suspension.Tests.Samples
{
    public partial class SimpleSuspensionPoint
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