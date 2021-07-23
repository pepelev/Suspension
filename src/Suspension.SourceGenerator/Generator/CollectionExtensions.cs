using System.Collections.Generic;
using System.Linq;

namespace Suspension.SourceGenerator.Generator
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<(T Left, T Right)> Pairwise<T>(this IEnumerable<T> sequence)
        {
            using var enumerator = sequence.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var first = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var second = enumerator.Current;
                yield return (first, second);

                first = second;
            }
        }

        public static IEnumerable<T> Without<T>(this IEnumerable<T> source, ICollection<T> subtrahend) =>
            source.Where(item => !subtrahend.Contains(item));
    }
}