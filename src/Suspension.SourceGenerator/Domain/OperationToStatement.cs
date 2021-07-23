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
    }
}