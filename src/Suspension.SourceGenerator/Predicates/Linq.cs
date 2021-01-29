using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Generator;

namespace Suspension.SourceGenerator.Predicates
{
    internal static class Linq
    {
        public static IEnumerable<T> That<T>(this IEnumerable<T> sequence, Predicate<T> predicate) =>
            sequence.Where(predicate.Match);

        public static IEnumerable<IOperation> That(this IEnumerable<IOperation> sequence, OperationVisitor<None, bool> visitor) =>
            sequence.That(new VisitorPredicate(visitor));

        public static IEnumerable<T> Select<T>(this IEnumerable<IOperation> sequence, OperationVisitor<None, T> visitor) =>
            sequence.Select(visitor.Visit);

        public static bool Contains<T>(this IEnumerable<T> sequence, Predicate<T> predicate) =>
            sequence.Any(predicate.Match);
    }
}