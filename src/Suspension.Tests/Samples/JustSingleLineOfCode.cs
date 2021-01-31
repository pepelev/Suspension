using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Suspension.Tests.Samples
{
    public partial class JustSingleLineOfCode
    {
        private Action action = () => { };

        [SetUp]
        public void SetUp()
        {
            action = Substitute.For<Action>();
        }

        [Suspendable]
        public static void Execute(Action action)
        {
            action();
        }

        [Test]
        public void Entry_Must_Not_Be_Completed()
        {
            var entry = new Coroutines.Execute.Entry(action);

            entry.Completed.Should().BeFalse();
        }

        [Test]
        public void Call_Action_On_Run()
        {
            var entry = new Coroutines.Execute.Entry(action);
            entry.Run();

            action.Received(1).Invoke();
        }

        [Test]
        public void Return_Completed_Coroutine_On_Run()
        {
            var entry = new Coroutines.Execute.Entry(action);
            var exit = entry.Run();

            exit.Completed.Should().BeTrue();
        }
    }
}