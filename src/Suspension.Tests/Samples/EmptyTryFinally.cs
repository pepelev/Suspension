using System;

namespace Suspension.Tests.Samples
{
    public partial class EmptyTryFinally
    {
        [Suspendable]
        public static void Execute(Action<string> action)
        {
            action("start");
            try
            {
            }
            finally
            {
                action("finally");
            }
        }
    }
}