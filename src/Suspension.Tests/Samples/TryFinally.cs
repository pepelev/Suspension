using System;

namespace Suspension.Tests.Samples
{
    public class TryFinally
    {
        //[Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
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