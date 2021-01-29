using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class OperationToStatement : OperationVisitor<Scope, StatementSyntax>
    {
        public override StatementSyntax DefaultVisit(IOperation operation, Scope scope) => ExpressionStatement(
            operation.Accept(new OperationToExpression(), scope)
        );

        public override StatementSyntax VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope scope)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    operation.Target.Accept(new OperationToExpression(), scope),
                    operation.Value.Accept(new OperationToExpression(), scope)
                )
            );
        }

        public override StatementSyntax VisitLocalReference(ILocalReferenceOperation operation, Scope scope) =>
            ExpressionStatement(
                new OperationToExpression().VisitLocalReference(operation, scope)
            );

        public override StatementSyntax VisitExpressionStatement(IExpressionStatementOperation operation, Scope scope)
        {
            var expression = operation.Operation.Accept(new OperationToExpression(), scope);
            return ExpressionStatement(expression);
        }
    }
}