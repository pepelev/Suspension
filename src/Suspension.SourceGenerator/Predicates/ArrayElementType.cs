using System;
using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class ArrayElementType : SymbolVisitor<ITypeSymbol>
    {
        public override ITypeSymbol DefaultVisit(ISymbol symbol) =>
            throw new NotSupportedException($"Symbol {symbol} is not array type");

        public override ITypeSymbol VisitArrayType(IArrayTypeSymbol symbol) => symbol.ElementType;
    }
}