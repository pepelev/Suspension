using System;

namespace Suspension.Tests.Samples
{
    public class Using
    {
        //[Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action, IDisposable resource)
        {
            using (resource)
            {
                if (argument())
                {
                    action("Ok");
                    return;
                }

                action("argument false");
            }
        }
    }
}