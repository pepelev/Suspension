using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator
{
    internal sealed class ScopeUsage : OperationVisitor<Scope, Scope>
    {
        public override Scope DefaultVisit(IOperation operation, Scope currentScope) =>
            currentScope;

        public override Scope VisitLocalReference(ILocalReferenceOperation operation, Scope currentScope)
            => currentScope.Add(
                new LocalValue(operation.Local)
            );

        public override Scope VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope argument) =>
            operation.Value.Accept(this, argument);

        public override Scope VisitExpressionStatement(IExpressionStatementOperation operation, Scope argument) =>
            operation.Operation.Accept(this, argument);

        public override Scope VisitInvocation(IInvocationOperation operation, Scope argument)
        {
            var scope = operation.Instance.Accept(this, argument);

            foreach (var operationArgument in operation.Arguments)
            {
                scope = operationArgument.Accept(this, scope);
            }

            return scope;
        }

        public override Scope VisitArgument(IArgumentOperation operation, Scope argument) =>
            operation.Value.Accept(this, argument);

        public override Scope VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope argument)
        {
            var scope = operation.Value.Accept(this, argument);
            return operation.Target.Accept(this, scope);
        }

        public override Scope VisitParameterReference(IParameterReferenceOperation operation, Scope argument) =>
            argument.Add(
                new ParameterValue(operation.Parameter)
            );
    }
}