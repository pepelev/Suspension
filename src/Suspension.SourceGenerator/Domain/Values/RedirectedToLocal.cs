using System.Collections.Generic;
using System.Linq;
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

        public override Identity Id => redirectedFrom.Id;
        public override ITypeSymbol Type => redirectedFrom.Type;
        public override string OriginalName => redirectedFrom.OriginalName;
        public override IEnumerable<string> OccupiedNames => redirectedFrom.OccupiedNames.Append(NewName);
        public override ExpressionSyntax Access => SyntaxFactory.IdentifierName(NewName);
        private string NewName => $"{OriginalName}{suffix}";
        public override string ToString() => $"{redirectedFrom} -> {NewName}";
    }
}