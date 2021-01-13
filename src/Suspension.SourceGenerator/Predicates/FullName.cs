using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
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
            (string?) ".",
            (IEnumerable<string?>) symbol.Accept(new FullSymbolName())
        );
    }
}