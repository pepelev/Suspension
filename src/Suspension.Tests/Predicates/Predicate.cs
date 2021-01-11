using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Suspension.Tests.Predicates
{
    internal abstract class Predicate<T>
    {
        public abstract bool Match(T argument);
    }

    internal sealed class FullName : Predicate<ISymbol>
    {
        private readonly string expectedName;

        public FullName(string expectedName)
        {
            this.expectedName = expectedName;
        }

        public override bool Match(ISymbol argument)
        {
            var actualName = Name(argument);
            return StringComparer.Ordinal.Equals(actualName, expectedName);
        }

        private string Name(ISymbol symbol) => string.Join(
            ".",
            symbol.Accept(new FullSymbolName())
        );
    }

    internal sealed class FullSymbolName : SymbolVisitor<ImmutableQueue<string>>
    {
        public override ImmutableQueue<string> DefaultVisit(ISymbol symbol) => Name(symbol);

        public override ImmutableQueue<string> VisitNamespace(INamespaceSymbol symbol) => symbol.IsGlobalNamespace
            ? ImmutableQueue<string>.Empty
            : Name(symbol);

        private ImmutableQueue<string> Name(ISymbol symbol) =>
            symbol.ContainingSymbol.Accept(this).Enqueue(symbol.Name);
    }
}
