using System;

namespace Suspension.Tests.Samples
{
    public partial class NestedTryFinally
    {
        [Suspendable]
        public static void Execute(Action<string> action)
        {
            action("start");
            try
            {
                action("outer try");
                try
                {
                    action("inner try");
                }
                finally
                {
                    action("inner finally");
                }
            }
            finally
            {
                action("outer finally");
            }
        }
    }
}