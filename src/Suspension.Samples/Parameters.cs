using System;
using System.Globalization;

namespace Suspension.Samples
{
    public sealed partial class Parameters
    {
        [Suspendable]
        public static void Regular(string value, Action<bool> equals)
        {
            var a = Equals(value, "str");
            equals(a);
        }

        [Suspendable]
        public static void Out(Action<int> action)
        {
            var d = 12;
            int.TryParse("123", NumberStyles.Integer, CultureInfo.InvariantCulture, out d);
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
            int.TryParse("19", NumberStyles.Integer, CultureInfo.InvariantCulture, out var d);
            action(d);
            Flow.Suspend("Parsed");
            d += 7;
            action(d);
        }
    }
}