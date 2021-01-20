using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class FullSymbolName : SymbolVisitor<string>
    {
        public override string DefaultVisit(ISymbol symbol)
        {
            var segments = symbol.Accept(new SymbolNameSegments());
            return string.Join(".", segments);
        }
    }
}