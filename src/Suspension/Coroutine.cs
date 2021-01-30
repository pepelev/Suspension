namespace Suspension
{
    public abstract class Coroutine<T>
    {
        public abstract bool Completed { get; }
        public abstract T Result { get; }
        public abstract Coroutine<T> Run();
    }
}