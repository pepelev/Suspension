using System;

namespace Suspension.Tests.Samples
{
    public static class JustSingleLineOfCode
    {
        [Suspendable]
        public static void Run(Action action)
        {
            action();
        }
    }
}