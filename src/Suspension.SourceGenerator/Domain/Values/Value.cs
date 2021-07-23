using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Suspension.SourceGenerator.Domain.Values
{
    internal abstract class Value
    {
        public abstract Identity Id { get; }
        public abstract ITypeSymbol Type { get; }
        public abstract string OriginalName { get; }
        public abstract IEnumerable<string> OccupiedNames { get; }
        public abstract ExpressionSyntax Access { get; }

        public abstract class Identity
        {
        }
    }
}