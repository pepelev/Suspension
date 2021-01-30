using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Suspension.SourceGenerator.Domain.Values
{
    internal sealed class FieldValue : Value
    {
        private readonly IFieldSymbol field;

        public FieldValue(IFieldSymbol field)
        {
            this.field = field;
        }

        public override ITypeSymbol Type => field.Type;
        public override string Name => field.Name;

        public override ExpressionSyntax Access => SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.ThisExpression(),
            SyntaxFactory.IdentifierName(Name)
        );


        private bool Equals(FieldValue other) =>
            SymbolEqualityComparer.Default.Equals(field, other.field);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || obj is FieldValue other && Equals(other);

        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(field);
        public override string ToString() => $"field: {Name}";
    }
}