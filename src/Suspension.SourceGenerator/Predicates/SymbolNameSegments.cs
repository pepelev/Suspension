using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class SymbolNameSegments : SymbolVisitor<ImmutableQueue<string>>
    {
        public override ImmutableQueue<string> DefaultVisit(ISymbol symbol) => Name(symbol);

        public override ImmutableQueue<string> VisitNamespace(INamespaceSymbol symbol) => symbol.IsGlobalNamespace
            ? ImmutableQueue<string>.Empty
            : Name(symbol);

        private ImmutableQueue<string> Name(ISymbol symbol) =>
            symbol.ContainingSymbol.Accept(this).Enqueue(symbol.Name);
    }
}