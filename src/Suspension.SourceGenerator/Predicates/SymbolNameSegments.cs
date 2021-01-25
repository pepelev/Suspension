using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class SymbolNameSegments : SymbolVisitor<ImmutableQueue<string>>
    {
        public override ImmutableQueue<string> DefaultVisit(ISymbol symbol) => Name(symbol);

        public override ImmutableQueue<string> VisitNamespace(INamespaceSymbol symbol) => symbol.IsGlobalNamespace
            ? ImmutableQueue<string>.Empty
            : Name(symbol);

        private ImmutableQueue<string> Name(ISymbol symbol) => Prefix(symbol).Enqueue(symbol.Name);

        private ImmutableQueue<string> Prefix(ISymbol symbol) => symbol.ContainingSymbol.Accept(this);

        public override ImmutableQueue<string> VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.IsGenericType)
            {
                var parameters = symbol.TypeArguments.Select(type => type.Accept(new FullSymbolName()));
                var name = $"{symbol.Name}<{string.Join(", ", parameters)}>";
                return Prefix(symbol).Enqueue(name);
            }

            return Name(symbol);
        }
    }
}