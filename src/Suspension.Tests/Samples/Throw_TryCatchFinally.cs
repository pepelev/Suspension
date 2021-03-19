using System;

namespace Suspension.Tests.Samples
{
    public class Throw_TryCatchFinally
    {
        public class Entry : Coroutine
        {
            public bool Completed => false;

            public Coroutine Run()
            {
                try
                {
                    Console.WriteLine("t-before");
                    return new Try();
                }
                catch
                {
                    Console.WriteLine("c-before");
                    return new Catch();
                }
            }
        }

        public class Try : Coroutine
        {
            public bool Completed => false;
            public Coroutine Run()
            {
                try
                {
                    Console.WriteLine("t-after");
                }
                catch
                {
                    Console.WriteLine("c-before");
                    return new Catch();
                }

                Console.WriteLine("t-after");
                return new Finally();
            }
        }

        public class Catch : Coroutine
        {
            public bool Completed => false;
            public Coroutine Run()
            {
                Exception e_e = null;
                try
                {
                    Console.WriteLine("c-after");
                }
                catch (Exception e)
                {
                    e_e = e;
                }
                Console.WriteLine("f-before");
                return e_e is null
                    ? new Finally()
                    : (Coroutine) new ThrowingAfter(e_e, new Finally());
            }
        }

        public class Finally : Coroutine
        {
            public bool Completed => false;
            public Coroutine Run()
            {
                Console.WriteLine();
                return new Exit();
            }
        }

        public class Exit : Coroutine
        {
            public bool Completed => true;
            public Coroutine Run() => throw new InvalidOperationException();
        }
    }

    public sealed class ThrowingAfter : Coroutine
    {
        private readonly Exception exception;
        private readonly Coroutine @finally;

        public ThrowingAfter(Exception exception, Coroutine @finally)
        {
            this.exception = exception;
            this.@finally = @finally;
        }

        public bool Completed => false;
        public Coroutine Run()
        {
            var next = @finally.Run();
            if (next.Completed)
                throw exception;

            return new ThrowingAfter(exception, next);
        }
    }
}