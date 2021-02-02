using Microsoft.CodeAnalysis;
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

        public override StatementSyntax VisitLocalReference(ILocalReferenceOperation operation, Scope scope) =>
            ExpressionStatement(
                new OperationToExpression().VisitLocalReference(operation, scope)
            );

        public override StatementSyntax VisitExpressionStatement(IExpressionStatementOperation operation, Scope scope) =>
            operation.Operation.Accept(this, scope);
    }
}