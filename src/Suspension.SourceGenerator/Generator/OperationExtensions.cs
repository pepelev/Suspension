using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Suspension.SourceGenerator.Generator
{
    internal static class OperationExtensions
    {
        public static T Accept<T>(this IOperation operation, OperationVisitor<None, T> visitor) =>
            operation.Accept(visitor, new None());

        public static T Visit<T>(this OperationVisitor<None, T> visitor, IOperation operation) =>
            operation.Accept(visitor, new None());

        public static Exception NotImplemented(this IOperation operation) => new NotImplementedException(
            $"Operation {operation.GetType().Name} with syntax {operation.Syntax} not supported yet {Environment.StackTrace.Replace("\r\n", " ")}"
        );
    }
}