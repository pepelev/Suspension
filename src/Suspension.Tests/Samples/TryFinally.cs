using System;

namespace Suspension.Tests.Samples
{
    public partial class TryFinally
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            action("start");
            try
            {
                if (argument())
                {
                    action("Ok");
                    return;
                }

                action("argument false");
            }
            finally
            {
                action("finally");
            }
        }
    }
}