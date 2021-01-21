using System;
using System.Collections.Generic;

namespace Suspension.SourceGenerator
{
    internal static class CollectionExtensions
    {
        public static Value Ensure<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Func<Value> value)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            var newValue = value();
            dictionary.Add(key, newValue);
            return newValue;
        }

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
                yield return (first, enumerator.Current);

                first = enumerator.Current;
            }
        }
    }
}