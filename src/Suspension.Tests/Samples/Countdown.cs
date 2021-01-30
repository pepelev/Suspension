using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
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

        [Suspendable]
        public void Report(Action<int> report, int count)
        {
            while (count > 0)
            {
                report(count);
                count--;
                Flow.Suspend("Cycle");
            }
        }

        [Test]
        public void Test()
        {
            Start(5).Select(coroutine => coroutine.Accept(new Print())).Should().Equal(
                "Entry: 5",
                "Cycle: 4",
                "Cycle: 3",
                "Cycle: 2",
                "Cycle: 1",
                "Cycle: 0",
                "Exit"
            );
        }

        [Test]
        public void Report0()
        {
            var report = Substitute.For<Action<int>>();
            Coroutines.Report coroutine = new Coroutines.Report.Entry(0, report);
            coroutine = coroutine.Run();

            coroutine.Completed.Should().BeTrue();
            report.DidNotReceive().Invoke(Arg.Any<int>());
        }

        [Test]
        public void Report1()
        {

            var report = Substitute.For<Action<int>>();
            Coroutines.Report coroutine = new Coroutines.Report.Entry(1, report);
            coroutine = coroutine.Run();
            coroutine = coroutine.Run();

            coroutine.Completed.Should().BeTrue();
            report.Received(1).Invoke(1);
        }

        [Test]
        public void Report2()
        {

            var report = Substitute.For<Action<int>>();
            Coroutines.Report coroutine = new Coroutines.Report.Entry(2, report);
            coroutine = coroutine.Run();
            coroutine = coroutine.Run();
            coroutine = coroutine.Run();

            coroutine.Completed.Should().BeTrue();
            report.Received(1).Invoke(2);
            report.Received(1).Invoke(1);
        }

        [Test]
        public void Report3()
        {
            StartReport(3).Select(coroutine => coroutine.Accept(new Print2())).Should().Equal(
                "Entry: 3",
                "Cycle: 2",
                "Cycle: 1",
                "Cycle: 0",
                "Exit"
            );
        }

        private IEnumerable<Coroutines.WriteToConsole> Start(int count)
        {
            Coroutines.WriteToConsole coroutine = new Coroutines.WriteToConsole.Entry(count);
            yield return coroutine;
            while (!coroutine.Completed)
            {
                coroutine = coroutine.Run();
                yield return coroutine;
            }
        }

        private IEnumerable<Coroutines.Report> StartReport(int count)
        {
            Coroutines.Report coroutine = new Coroutines.Report.Entry(count, number => {});
            yield return coroutine;
            while (!coroutine.Completed)
            {
                coroutine = coroutine.Run();
                yield return coroutine;
            }
        }

        private sealed class Print : Coroutines.WriteToConsole.Visitor<string>
        {
            public override string VisitEntry(int count) => $"Entry: {count}";
            public override string VisitExit() => "Exit";
            public override string VisitCycle(int count) => $"Cycle: {count}";
        }

        private sealed class Print2 : Coroutines.Report.Visitor<string>
        {
            public override string VisitEntry(int count, Action<int> report) => $"Entry: {count}";
            public override string VisitExit() => "Exit";
            public override string VisitCycle(int count, Action<int> report) => $"Cycle: {count}";
        }
    }
}