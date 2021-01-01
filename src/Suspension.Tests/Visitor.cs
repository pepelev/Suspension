using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Suspension.Tests
{
    public sealed class Visitor : CSharpSyntaxVisitor<string>
    {
        private readonly SemanticModel model;

        public Visitor(SemanticModel model)
        {
            this.model = model;
        }

        public override string VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var symbolInfo = model.GetSymbolInfo(node.Declaration.Type);
            if (symbolInfo.Symbol is {} symbol)
            {
                var symbolVisitor = new SymVisitor();
                return symbol.Accept(symbolVisitor) + ": " + string.Join(
                    " ",
                    node.Declaration.Variables.Select(variable => variable.Identifier.Text)
                );
            }

            return string.Join(
                " ",
                node.Declaration.Variables.Select(variable => variable.Identifier.Text)
            );
        }

        public override string VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return "ExpressionStatementSyntax";
        }

        public override string DefaultVisit(SyntaxNode node) => "Fail";
    }
}