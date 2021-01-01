using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.Tests
{
    public sealed class Walker : CSharpSyntaxWalker
    {
        private readonly SemanticModel model;

        public Walker(SemanticModel model)
        {
            this.model = model;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var symbol = model.GetDeclaredSymbol(node);
            if (symbol is {} method)
            {
                if (method.Name == "Run")
                {
                    var flow = ControlFlowGraph.Create(node, model);
                    Go(node, symbol);
                }
            }
            base.VisitMethodDeclaration(node);
        }

        private void Go(MethodDeclarationSyntax node, IMethodSymbol symbol)
        {
            if (node.Body is {} body)
            {
                var visitor = new Visitor(model);

                var @join = string.Join(
                    Environment.NewLine,
                    body.Statements.Select(visitor.Visit)
                );
            }
        }
    }
}