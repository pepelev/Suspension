using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator
{
    internal sealed class ScopeVisitor : OperationVisitor<Domain.Scope, Domain.Scope>
    {
        public override Domain.Scope DefaultVisit(IOperation operation, Domain.Scope currentScope) =>
            currentScope;

        public override Domain.Scope VisitLocalReference(ILocalReferenceOperation operation, Domain.Scope currentScope)
            => currentScope.Add(
                new LocalVariable(operation.Local)
            );
    }
}