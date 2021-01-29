using System;

namespace Suspension.Tests.Samples
{
    public class SingleTryCatch
    {
        //[Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            try
            {
                if (argument())
                {
                    action("Ok");
                }
                else
                {
                    action("No ok");
                }
            }
            catch (Exception e)
            {
                action($"throw {e.Message}");
            }
        }
    }
}