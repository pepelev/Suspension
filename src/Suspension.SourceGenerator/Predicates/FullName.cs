using System;
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
            var actualName = argument.Accept(new FullSymbolName());
            return StringComparer.Ordinal.Equals(actualName, expectedName);
        }
    }
}