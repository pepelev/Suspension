namespace Suspension
{
    public interface Coroutine<out T>
    {
        bool Completed { get; }
        T Result { get; }
        Coroutine<T> Run();
    }
}