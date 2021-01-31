namespace Suspension
{
    public interface Coroutine<out T>
    {
        bool Completed { get; }
        Coroutine<T> Run();
    }
}