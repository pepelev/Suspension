using System.Linq;
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

        public override Scope VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope currentScope)
        {
            var newScope = operation.Target.Accept(this, currentScope);
            return operation.Value.Accept(this, newScope);
        }

        public override Scope VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation, Scope currentScope)
        {
            var newScope = operation.Target.Accept(this, currentScope);
            return operation.Value.Accept(this, newScope);
        }

        public override Scope VisitTuple(ITupleOperation operation, Scope currentScope) =>
            operation.Elements.Aggregate(currentScope, (scope, element) => element.Accept(this, scope));

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
            currentScope.Union(
                new ParameterValue(operation.Parameter)
            );

        public override Scope VisitLocalReference(ILocalReferenceOperation operation, Scope currentScope)
            => currentScope.Union(
                new LocalValue(operation.Local)
            );

        public override Scope VisitArrayElementReference(IArrayElementReferenceOperation operation, Scope currentScope)
        {
            var newScope = operation.ArrayReference.Accept(this, currentScope);
            return operation.Indices.Aggregate(newScope, (scope, index) => index.Accept(this, scope));
        }

        public override Scope VisitUnaryOperator(IUnaryOperation operation, Scope currentScope) =>
            operation.Operand.Accept(this, currentScope);

        public override Scope VisitBinaryOperator(IBinaryOperation operation, Scope currentScope)
        {
            var result = operation.LeftOperand.Accept(this, currentScope);
            return operation.RightOperand.Accept(this, result);
        }

        public override Scope VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Scope currentScope) =>
            operation.Target.Accept(this, currentScope);

        public override Scope VisitObjectCreation(IObjectCreationOperation operation, Scope currentScope)
        {
            var scope = currentScope;
            foreach (var argument in operation.Arguments)
            {
                scope = argument.Accept(this, scope);
            }

            return scope;
        }

        public override Scope VisitArrayCreation(IArrayCreationOperation operation, Scope currentScope)
        {
            var newScope = operation.DimensionSizes.Aggregate(
                currentScope,
                (scope, size) => size.Accept(this, scope)
            );
            return operation.Initializer switch
            {
                { } initializer => initializer.Accept(this, newScope),
                null => newScope
            };
        }

        public override Scope VisitArrayInitializer(IArrayInitializerOperation operation, Scope currentScope) =>
            operation.ElementValues.Aggregate(
                currentScope,
                (scope, element) => element.Accept(this, scope)
            );

        public override Scope VisitConversion(IConversionOperation operation, Scope currentScope) =>
            operation.Operand.Accept(this, currentScope);

        public override Scope VisitDiscardOperation(IDiscardOperation operation, Scope currentScope) => currentScope;

        public override Scope VisitDeclarationExpression(IDeclarationExpressionOperation operation, Scope currentScope)
            => currentScope;

        public override Scope VisitFieldReference(IFieldReferenceOperation operation, Scope currentScope)
        {
            // todo support fields
            return currentScope;
        }

        public override Scope VisitPropertyReference(IPropertyReferenceOperation operation, Scope currentScope)
        {
            // todo support fields
            return currentScope;
        }
    }
}