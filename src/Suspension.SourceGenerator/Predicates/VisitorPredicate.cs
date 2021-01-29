using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Generator;

namespace Suspension.SourceGenerator.Predicates
{
    internal sealed class VisitorPredicate : Predicate<IOperation>
    {
        private readonly OperationVisitor<None, bool> visitor;

        public VisitorPredicate(OperationVisitor<None, bool> visitor)
        {
            this.visitor = visitor;
        }

        public override bool Match(IOperation argument) => argument.Accept(visitor);
    }
}