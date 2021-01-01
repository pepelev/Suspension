using System.Linq;
using Microsoft.CodeAnalysis;

namespace Suspension.Tests
{
    public class SymVisitor : SymbolVisitor<string>
    {
        public override string VisitArrayType(IArrayTypeSymbol symbol)
        {
            return Visit(symbol.ElementType) + $"[{string.Join("", Enumerable.Repeat(",", symbol.Rank - 1))}]";
        }

        public override string DefaultVisit(ISymbol symbol)
        {
            return $"Default: {symbol}";
        }

        public override string VisitNamedType(INamedTypeSymbol symbol)
        {
            return symbol.Name;
        }
    }
}