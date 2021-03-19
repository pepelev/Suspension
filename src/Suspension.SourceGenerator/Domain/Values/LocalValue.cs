using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Suspension.SourceGenerator.Domain.Values
{
    internal sealed class LocalValue : Value
    {
        private readonly ILocalSymbol local;

        public LocalValue(ILocalSymbol local)
        {
            this.local = local;
        }

        public override Value.Identity Id => new Identity(local);
        public override ITypeSymbol Type => local.Type;
        public override string OriginalName => local.Name;
        public override IEnumerable<string> OccupiedNames => new[] {OriginalName};
        public override ExpressionSyntax Access => SyntaxFactory.IdentifierName(OriginalName);
        public override string ToString() => $"local: {local.Name}";

        private new sealed class Identity : Value.Identity
        {
            private static SymbolEqualityComparer Equality => SymbolEqualityComparer.Default;
            private readonly ILocalSymbol local;

            public Identity(ILocalSymbol local)
            {
                this.local = local;
            }

            private bool Equals(Identity other) => Equality.Equals(local, other.local);

            public override bool Equals(object obj) =>
                ReferenceEquals(this, obj) ||
                obj is Identity other && Equals(other);

            public override int GetHashCode() => Equality.GetHashCode(local);
        }
    }
}