using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Predicates;

namespace Suspension.SourceGenerator
{
    internal static class SuspensionPoint
    {
        public sealed class Is : OperationVisitor<None, bool>
        {
            public override bool DefaultVisit(IOperation operation, None _) => false;

            public override bool VisitExpressionStatement(IExpressionStatementOperation operation, None _) =>
                operation.Operation.Accept(this);

            public override bool VisitInvocation(IInvocationOperation operation, None _)
            {
                var predicate = new FullName("Suspension.Flow.Suspend");
                return predicate.Match(operation.TargetMethod);
            }
        }

        public sealed class Name : OperationVisitor<None, string>
        {
            public override string DefaultVisit(IOperation operation, None _)
            {
                throw new InvalidOperationException($"{operation} is not suspension point");
            }

            public override string VisitExpressionStatement(IExpressionStatementOperation operation, None _) =>
                operation.Operation.Accept(this);

            public override string VisitInvocation(IInvocationOperation operation, None _)
            {
                var argument = operation.Arguments switch
                {
                    { Length: 1 } arguments => arguments[0],
                    _ => throw new InvalidOperationException($"{operation} must have single argument")
                };
                return argument.Value.ConstantValue switch
                {
                    { HasValue: true, Value: string name } => name,
                    _ => throw new InvalidOperationException($"Suspension point {operation} must have name as compile time constant")
                };
            }
        }
    }
}