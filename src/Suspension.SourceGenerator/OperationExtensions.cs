using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Suspension.SourceGenerator
{
    internal static class OperationExtensions
    {
        public static T Accept<T>(this IOperation operation, OperationVisitor<None, T> visitor) =>
            operation.Accept(visitor, new None());

        public static T Visit<T>(this OperationVisitor<None, T> visitor, IOperation operation) =>
            operation.Accept(visitor, new None());
    }
}