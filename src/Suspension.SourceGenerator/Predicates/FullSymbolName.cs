using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class FullSymbolName : SymbolVisitor<string>
    {
        public override string DefaultVisit(ISymbol symbol)
        {
            var segments = symbol.Accept(new SymbolNameSegments());
            var joinedSegments = string.Join(".", segments);
            return symbol.Accept(new IsNamespace())
                ? joinedSegments
                : $"global::{joinedSegments}";
        }
    }
}