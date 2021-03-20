using System;

namespace Suspension.Tests.Samples
{
    public partial class NestedTryCatch
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
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
                    action("inner catch");
                }
            }
            catch
            {
                action("throw");
            }
        }
    }
}