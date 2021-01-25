﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator
{
    internal sealed class ScopeDeclaration : OperationVisitor<Scope, Scope>
    {
        public override Scope DefaultVisit(IOperation operation, Scope currentScope) =>
            throw new InvalidOperationException($"Unsupported operation {operation}");

        public override Scope VisitLocalReference(ILocalReferenceOperation operation, Scope currentScope)
            => currentScope.Add(
                new LocalValue(operation.Local)
            );

        public override Scope VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope currentScope) =>
            operation.Target.Accept(this, currentScope);

        public override Scope VisitExpressionStatement(IExpressionStatementOperation operation, Scope currentScope) =>
            operation.Operation.Accept(this, currentScope);

        public override Scope VisitInvocation(IInvocationOperation operation, Scope currentScope) => currentScope;
        public override Scope VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope currentScope) => currentScope;
    }
}