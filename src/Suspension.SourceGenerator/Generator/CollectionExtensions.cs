﻿using System.Collections.Generic;

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
                yield return (first, enumerator.Current);

                first = enumerator.Current;
            }
        }
    }
}