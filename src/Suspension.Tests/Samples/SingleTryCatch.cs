using System;

namespace Suspension.Tests.Samples
{
    public partial class SingleTryCatch
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