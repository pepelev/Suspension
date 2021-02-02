using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class ScopeDeclaration : OperationVisitor<Scope, Scope>
    {
        public override Scope DefaultVisit(IOperation operation, Scope currentScope) =>
            throw operation.NotImplemented();

        public override Scope VisitLocalReference(ILocalReferenceOperation operation, Scope currentScope)
            => currentScope.Add(
                new LocalValue(operation.Local)
            );

        public override Scope VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope currentScope) =>
            operation.Target.Accept(this, currentScope);

        public override Scope VisitExpressionStatement(IExpressionStatementOperation operation, Scope currentScope) =>
            operation.Operation.Accept(this, currentScope);

        public override Scope VisitInvocation(IInvocationOperation operation, Scope currentScope)
        {
            var scope = operation.Instance switch
            {
                null => currentScope,
                { } instance => instance.Accept(this, currentScope)
            };
            foreach (var argument in operation.Arguments)
            {
                scope = argument.Accept(this, scope);
            }

            return scope;
        }

        public override Scope VisitArgument(IArgumentOperation operation, Scope currentScope) =>
            operation.Value.Accept(this, currentScope);

        public override Scope VisitLiteral(ILiteralOperation operation, Scope currentScope) => currentScope;

        public override Scope VisitParameterReference(IParameterReferenceOperation operation, Scope currentScope) =>
            currentScope;

        public override Scope VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope currentScope)
            => currentScope;

        public override Scope VisitBinaryOperator(IBinaryOperation operation, Scope currentScope) => currentScope;

        public override Scope VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Scope currentScope)
            => currentScope;

        public override Scope VisitObjectCreation(IObjectCreationOperation operation, Scope currentScope) => currentScope;

        public override Scope VisitConversion(IConversionOperation operation, Scope currentScope) =>
            operation.Operand.Accept(this, currentScope);

        public override Scope VisitDiscardOperation(IDiscardOperation operation, Scope currentScope) => currentScope;

        public override Scope VisitDeclarationExpression(IDeclarationExpressionOperation operation, Scope currentScope) =>
            operation.Expression.Accept(this, currentScope);


        public override Scope VisitFieldReference(IFieldReferenceOperation operation, Scope currentScope) =>
            currentScope;

        public override Scope VisitPropertyReference(IPropertyReferenceOperation operation, Scope currentScope) =>
            currentScope;
    }
}