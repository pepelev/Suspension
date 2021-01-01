namespace Suspension.Tests
{
    public abstract class Coroutine<T>
    {
        public abstract bool Completed { get; }
        public abstract T Result { get; }
        public abstract Coroutine<T> Run();

        public abstract class Continuation<Argument>
        {
            public abstract Coroutine<T> Run(Argument argument);
        }
    }
}