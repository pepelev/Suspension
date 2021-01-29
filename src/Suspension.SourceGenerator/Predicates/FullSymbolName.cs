using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class FullSymbolName : SymbolVisitor<string>
    {
        public override string DefaultVisit(ISymbol symbol)
        {
            var noGlobalSymbolName = symbol.Accept(new NoGlobalFullSymbolName());
            return symbol.Accept(new IsNamespace())
                ? noGlobalSymbolName
                : $"global::{noGlobalSymbolName}";
        }
    }
}