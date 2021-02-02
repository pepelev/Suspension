#pragma warning disable 219
namespace Suspension.Tests.Samples
{
    public sealed partial class New
    {
        public New(out int a)
        {
            a = 10;
        }

        [Suspendable]
        public static void String()
        {
            var s = new string('p', 42);
        }

        [Suspendable]
        public static void OutParameter()
        {
            var s = new New(out _);
        }

        [Suspendable]
        public static void Object()
        {
            var o = new object();
        }

        [Suspendable]
        public static void NullableInt()
        {
            var i = new int?(57);
        }

        [Suspendable]
        public static void Int()
        {
            var i = new int();
        }
    }
}