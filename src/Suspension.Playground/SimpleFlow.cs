using System;

namespace Suspension.Playground
{
    public partial class SimpleFlow
    {
        [Suspendable]
        public static void Execute(Action<string> action)
        {
            action("1");
            Flow.Suspend("First");
            action("2");
            Flow.Suspend("Second");
            action("3");
        }
    }
}