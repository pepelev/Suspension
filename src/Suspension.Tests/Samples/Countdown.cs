using System;
using NUnit.Framework;

namespace Suspension.Tests.Samples
{
    public partial class Countdown
    {
        [Suspendable]
        public void WriteToConsole(int count)
        {
            while (count > 0)
            {
                Console.WriteLine(count);
                count--;
                Flow.Suspend("Cycle");
            }
        }

        [Test]
        public void Test()
        {
            Coroutines.WriteToConsole coroutine = new Coroutines.WriteToConsole.Entry(10);
            while (!coroutine.Completed)
            {
                Console.WriteLine(
                    coroutine.Accept(new Print())
                );
                coroutine = coroutine.Run();
            }

            Console.WriteLine(
                coroutine.Accept(new Print())
            );
        }

        private sealed class Print : Coroutines.WriteToConsole.Visitor<string>
        {
            public override string VisitEntry(int count) => $"Entry: {count}";
            public override string VisitExit() => "Exit";
            public override string VisitCycle(int count) => $"Cycle: {count}";
        }
    }
}