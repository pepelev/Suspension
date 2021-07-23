namespace Suspension.SourceGenerator.Generator
{
    internal static class NumberExtensions
    {
        public static bool Between(this int number, int left, int right) => left <= number && number <= right;
    }
}