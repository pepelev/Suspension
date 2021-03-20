using System;

namespace Suspension.Tests.Samples
{
    public partial class NestedTryCatchWithEmptyCatch
    {
        [Suspendable]
        public static void Execute(Action<string> action)
        {
            action("start");
            try
            {
                try
                {
                    action("inner try");
                }
                catch
                {
                    // ignored
                }
            }
            catch
            {
                action("throw");
            }
        }
    }
}