using System;
using NSubstitute;
using NUnit.Framework;

namespace Suspension.Tests.Samples
{
    public sealed partial class Parameters
    {
        [Suspendable]
        public static void Regular(string value)
        {
            var a = Equals(value, "str");
        }

        [Suspendable]
        public static void Out(Action<int> action)
        {
            var d = 12;
            int.TryParse("123", out d);
            Flow.Suspend("Middle");
            action(d);
        }

        [Suspendable]
        public static void OutDiscard()
        {
            int.TryParse("153", out _);
        }

        [Suspendable]
        public static void OutDeclaration(Action<int> action)
        {
            int.TryParse("193", out var d);
            Flow.Suspend("Middle");
            action(d);
        }

        [Test]
        public void Test()
        {
            var spy = Substitute.For<Action<int>>();
            var entry = new Coroutines.Out.Entry(spy);
            var middle = entry.Run();
            middle.Run();

            spy.Received(1).Invoke(123);
        }
    }
}