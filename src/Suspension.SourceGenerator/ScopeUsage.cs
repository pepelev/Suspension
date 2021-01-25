using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator
{
    internal sealed class ScopeUsage : OperationVisitor<Scope, Scope>
    {
        public override Scope DefaultVisit(IOperation operation, Scope currentScope) =>
            throw new InvalidOperationException($"Unsupported operation {operation}");

        public override Scope VisitLocalReference(ILocalReferenceOperation operation, Scope currentScope)
            => currentScope.Add(
                new LocalValue(operation.Local)
            );

        public override Scope VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope currentScope) =>
            operation.Value.Accept(this, currentScope);

        public override Scope VisitExpressionStatement(IExpressionStatementOperation operation, Scope currentScope) =>
            operation.Operation.Accept(this, currentScope);

        public override Scope VisitInvocation(IInvocationOperation operation, Scope currentScope)
        {
            var scope = operation.Instance.Accept(this, currentScope);

            foreach (var operationArgument in operation.Arguments)
            {
                scope = operationArgument.Accept(this, scope);
            }

            return scope;
        }

        public override Scope VisitArgument(IArgumentOperation operation, Scope currentScope) =>
            operation.Value.Accept(this, currentScope);

        public override Scope VisitLiteral(ILiteralOperation operation, Scope currentScope) => currentScope;

        public override Scope VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope currentScope)
        {
            var scope = operation.Value.Accept(this, currentScope);
            return operation.Target.Accept(this, scope);
        }

        public override Scope VisitParameterReference(IParameterReferenceOperation operation, Scope currentScope) =>
            currentScope.Add(
                new ParameterValue(operation.Parameter)
            );
    }
}