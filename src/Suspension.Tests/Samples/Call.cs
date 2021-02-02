using System;

namespace Suspension.Tests.Samples
{
    public sealed partial class Call
    {
        [Suspendable]
        public static void StaticMethod()
        {
            Console.WriteLine("Hello");
        }

        private static void Do()
        {
            Console.WriteLine("Do");
        }

        [Suspendable]
        public static void InstanceMethod(object a)
        {
            var hashCode = a.GetHashCode();
        }

        [Suspendable]
        public static void Delegate(Action a)
        {
            a();
        }

        [Suspendable]
        public static void DelegateWithParameter(Action<string> a)
        {
            a("hello");
        }

        [Suspendable]
        public static void InvokeDelegate(Action a)
        {
            a.Invoke();
        }

        [Suspendable]
        public static void InvokeDelegateWithParameter(Action<string> a)
        {
            a.Invoke("Hello");
        }

        [Suspendable]
        public static void PrivateStaticMethod()
        {
            Do();
        }
    }
}