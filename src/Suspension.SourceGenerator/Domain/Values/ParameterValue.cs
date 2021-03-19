using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Domain.Values
{
    internal sealed class ParameterValue : Value
    {
        private readonly IParameterSymbol parameter;

        public ParameterValue(IParameterSymbol parameter)
        {
            this.parameter = parameter;
        }

        public override Value.Identity Id => new Identity(parameter);
        public override ITypeSymbol Type => parameter.Type;
        public override string OriginalName => parameter.Name;
        public override IEnumerable<string> OccupiedNames => new[] {OriginalName};

        public override ExpressionSyntax Access => MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            ThisExpression(),
            IdentifierName(OriginalName)
        );

        public override string ToString() => $"parameter: {parameter.Name}";



        private new sealed class Identity : Value.Identity
        {
            private static SymbolEqualityComparer Equality => SymbolEqualityComparer.Default;
            private readonly IParameterSymbol parameter;

            public Identity(IParameterSymbol parameter)
            {
                this.parameter = parameter;
            }

            private bool Equals(Identity other) => Equality.Equals(parameter, other.parameter);

            public override bool Equals(object obj) =>
                ReferenceEquals(this, obj) || obj is Identity other && Equals(other);

            public override int GetHashCode() => Equality.GetHashCode(parameter);
        }
    }
}