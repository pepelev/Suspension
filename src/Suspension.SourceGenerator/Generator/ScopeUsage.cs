using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class ScopeUsage : OperationVisitor<Scope, Scope>
    {
        public override Scope DefaultVisit(IOperation operation, Scope currentScope) =>
            throw operation.NotImplemented();

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
            var scope = operation.Instance switch
            {
                null => currentScope,
                var instance => instance.Accept(this, currentScope)
            };

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

        public override Scope VisitBinaryOperator(IBinaryOperation operation, Scope currentScope)
        {
            var result = operation.LeftOperand.Accept(this, currentScope);
            return operation.RightOperand.Accept(this, result);
        }

        public override Scope VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Scope currentScope) =>
            operation.Target.Accept(this, currentScope);

        public override Scope VisitFieldReference(IFieldReferenceOperation operation, Scope currentScope) =>
            operation.Field switch
            {
                {IsStatic: true} => currentScope,
                var field => currentScope.Add(new FieldValue(field))
            };
    }
}