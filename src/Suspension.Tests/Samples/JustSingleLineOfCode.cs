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
        public void Throw_Exception_On_Result()
        {
            var entry = new Coroutines.Execute.Entry(action);

            Assert.Throws<InvalidOperationException>(
                () => _ = entry.Result
            );
        }

        [Test]
        public void Call_Action_On_Run()
        {
            var entry = new Coroutines.Execute.Entry(action);
            entry.Run();

            action.Received(1).Invoke();
        }

        [Test]
        public void Return_Completed_Coroutine()
        {
            var entry = new Coroutines.Execute.Entry(action);
            var exit = entry.Run();

            exit.Completed.Should().BeTrue();
        }

        [Test]
        public void Give_Result_When_Completed()
        {
            var entry = new Coroutines.Execute.Entry(action);
            var exit = entry.Run();

            Assert.DoesNotThrow(
                () => _ = exit.Result
            );
        }
    }
}