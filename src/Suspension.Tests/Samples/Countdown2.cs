using System;
using System.Globalization;

namespace Suspension.Tests.Samples
{
    public class Countdown2
    {
        public void WriteToConsole(int count)
        {
            while (count > 0)
            {
                Console.WriteLine(count);
                count--;
                Flow.Suspend("Cycle");
            }
        }

        public void Declare(int count)
        {
            if (count > 42)
            {
                Console.WriteLine("d");
            }

            var b = 10;

            while (b < count)
            {
                Console.WriteLine(b);
                b++;
            }
        }

        public void Declare2(int count)
        {
            if (count > 42)
            {
                var p = 17;
                Console.WriteLine("d");
            }

            var b = 10;

            while (b < count)
            {
                Console.WriteLine(b);
                b++;
            }
        }

        public static void Out()
        {
            var d = 12;
            int.TryParse("123", NumberStyles.Integer, CultureInfo.InvariantCulture, out d);
            Flow.Suspend("Middle");
        }
    }
}