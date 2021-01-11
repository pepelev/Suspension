using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Suspension.Tests.Predicates
{
    internal sealed class HasAttribute : Predicate<MethodDeclarationSyntax>
    {
        private readonly SemanticModel semantic;
        private readonly Predicate<ISymbol> predicate;

        public HasAttribute(SemanticModel semantic, Predicate<ISymbol> predicate)
        {
            this.semantic = semantic;
            this.predicate = predicate;
        }

        public override bool Match(MethodDeclarationSyntax method) => semantic
                .GetDeclaredSymbol(method)
                .GetAttributes()
                .Any(attribute => predicate.Match(attribute.AttributeClass));
    }
}
