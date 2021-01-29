using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Suspension.SourceGenerator.Domain.Values
{
    internal abstract class Value
    {
        public abstract ITypeSymbol Type { get; }
        public abstract string Name { get; }
        public abstract ExpressionSyntax Access { get; }
    }
}