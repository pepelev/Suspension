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

        public override ITypeSymbol Type => parameter.Type;
        public override string Name => parameter.Name;

        public override ExpressionSyntax Access => MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            ThisExpression(),
            IdentifierName(Name)
        );

        private bool Equals(ParameterValue other) =>
            SymbolEqualityComparer.Default.Equals(parameter, other.parameter);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || obj is ParameterValue other && Equals(other);

        public override int GetHashCode() =>
            SymbolEqualityComparer.Default.GetHashCode(parameter);

        public override string ToString() => $"parameter: {parameter.Name}";
    }
}