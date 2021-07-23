using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class FullSymbolName : SymbolVisitor<string>
    {
        public static SymbolVisitor<string> WithGlobal { get; } = new FullSymbolName();
        public static SymbolVisitor<string> WithoutGlobal { get; } = new NoGlobalFullSymbolName();

        private FullSymbolName()
        {
        }

        public override string DefaultVisit(ISymbol symbol)
        {
            var noGlobalSymbolName = symbol.Accept(WithoutGlobal);
            return symbol.Accept(new IsNamespace())
                ? noGlobalSymbolName
                : $"global::{noGlobalSymbolName}";
        }

        private sealed class NoGlobalFullSymbolName : SymbolVisitor<string>
        {
            public override string DefaultVisit(ISymbol symbol)
            {
                var segments = symbol.Accept(new SymbolNameSegments());
                return string.Join(".", segments);
            }
        }
    }
}