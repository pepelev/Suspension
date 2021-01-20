using System.Collections.Generic;
using System.Linq;

namespace Suspension.SourceGenerator.Predicates
{
    internal static class Linq
    {
        public static IEnumerable<T> That<T>(this IEnumerable<T> sequence, Predicate<T> predicate) =>
            sequence.Where(predicate.Match);

        public static bool Contains<T>(this IEnumerable<T> sequence, Predicate<T> predicate) =>
            sequence.Any(predicate.Match);
    }
}