namespace Suspension.SourceGenerator.Predicates
{
    internal abstract class Predicate<T>
    {
        public abstract bool Match(T argument);
    }
}