using System;

namespace Suspension
{
    public static class Flow
    {
        public static void Suspend(string name)
        {
        }
    }

    public static class Flow<T>
    {
        public static T Wait(string name)
        {
            throw new InvalidOperationException(
                "This method is a markup, so it must not be called directly"
            );
        }
    }
}