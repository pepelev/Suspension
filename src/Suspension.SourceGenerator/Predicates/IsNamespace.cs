using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class IsNamespace : SymbolVisitor<bool>
    {
        public override bool DefaultVisit(ISymbol symbol) => false;
        public override bool VisitNamespace(INamespaceSymbol symbol) => true;
    }
}