using System;

namespace Suspension.Tests.Samples.JustSingleLineOfCode
{
    public static partial class Class
    {
        [Suspendable]
        public static void Execute(Action action)
        {
            action();
        }
    }
}