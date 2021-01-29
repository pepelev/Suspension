using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Suspension.SourceGenerator.Domain.Values
{
    internal sealed class RedirectedToLocal : Value
    {
        private readonly Value redirectedFrom;
        private readonly string suffix;

        public RedirectedToLocal(Value redirectedFrom, string suffix)
        {
            this.redirectedFrom = redirectedFrom;
            this.suffix = suffix;
        }

        public override ITypeSymbol Type => redirectedFrom.Type;
        public override string Name => redirectedFrom.Name;
        public override ExpressionSyntax Access => SyntaxFactory.IdentifierName(NewName);
        private string NewName => $"{Name}{suffix}";

        private bool Equals(Value other) =>
            other.Equals(redirectedFrom);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || obj is Value other && Equals(other);

        public override int GetHashCode() => redirectedFrom.GetHashCode();
        public override string ToString() => $"{redirectedFrom} -> {NewName}";
    }
}