using System;
using static Suspension.Flow;

namespace Suspension.Tests
{
    public sealed class Example
    {
        public void Run(Action<int> action) // start
        {
            action(1);
            Suspend("One"); // 1
            action(2);
            action(3);
            Suspend("Two");
            action(4);
        } // finish

        public sealed class Start : Coroutine<None>
        {
            private readonly Action<int> action;

            public Start(Action<int> action)
            {
                this.action = action;
            }

            public override bool Completed => false;
            public override None Result => throw new InvalidOperationException();

            public override Coroutine<None> Run()
            {
                action(1);
                return new One(action);
            }
        }

        private sealed class One : Coroutine<None>
        {
            private readonly Action<int> action;

            public One(Action<int> action)
            {
                this.action = action;
            }

            public override bool Completed => false;
            public override None Result => throw new InvalidOperationException();

            public override Coroutine<None> Run()
            {
                action(2);
                action(3);
                return new Two(action);
            }
        }

        private sealed class Two : Coroutine<None>
        {
            private readonly Action<int> action;

            public Two(Action<int> action)
            {
                this.action = action;
            }

            public override bool Completed => false;
            public override None Result => throw new InvalidOperationException();

            public override Coroutine<None> Run()
            {
                action(4);
                return new Finish();
            }
        }

        private sealed class Finish : Coroutine<None>
        {
            public override bool Completed => true;
            public override None Result => new None();
            public override Coroutine<None> Run() => throw new InvalidOperationException();
        }

        private void RunUsage()
        {
            Coroutine<None> coroutine = new Start(Console.WriteLine);
            while (!coroutine.Completed)
            {
                coroutine = coroutine.Run();
            }
        }
    }
}