namespace Suspension
{
    public interface Coroutine
    {
        bool Completed { get; }
        Coroutine Run();
    }
}