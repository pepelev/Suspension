using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Predicates;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class OperationToStatement : OperationVisitor<Scope, StatementSyntax>
    {
        public override StatementSyntax DefaultVisit(IOperation operation, Scope scope) => ExpressionStatement(
            operation.Accept(new OperationToExpression(), scope)
        );

        public override StatementSyntax VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope scope)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    operation.Target.Accept(new OperationToExpression(), scope),
                    operation.Value.Accept(new OperationToExpression(), scope)
                )
            );
        }

        public override StatementSyntax VisitLocalReference(ILocalReferenceOperation operation, Scope scope) =>
            ExpressionStatement(
                new OperationToExpression().VisitLocalReference(operation, scope)
            );

        public override StatementSyntax VisitExpressionStatement(IExpressionStatementOperation operation, Scope scope)
        {
            var expression = operation.Operation.Accept(new OperationToExpression(), scope);
            return ExpressionStatement(expression);
        }
    }

    internal sealed class OperationToExpression : OperationVisitor<Scope, ExpressionSyntax>
    {
        public override ExpressionSyntax DefaultVisit(IOperation operation, Scope argument) =>
            throw new NotSupportedException($"Operation {operation} not supported");

        public override ExpressionSyntax VisitInvocation(IInvocationOperation operation, Scope scope) =>
            InvocationExpression(
                operation.Instance.Accept(this, scope),
                ArgumentList(
                    SeparatedList(
                        operation.Arguments.Select(
                            argument => Argument(argument.Value.Accept(this, scope))
                        )
                    )
                )
            );

        public override ExpressionSyntax VisitParameterReference(IParameterReferenceOperation operation, Scope scope)
        {
            var parameter = new ParameterValue(operation.Parameter);
            return scope.Find(parameter).Access;
        }

        public override ExpressionSyntax VisitLocalReference(ILocalReferenceOperation operation, Scope scope)
        {
            var local = new LocalValue(operation.Local);
            if (scope.Contains(local))
            {
                return scope.Find(local).Access;
            }

            //return LocalDeclarationStatement(
            //    List<AttributeListSyntax>(),
            //    TokenList(),
            //    VariableDeclaration(
            //        IdentifierName(local.Type.Accept(new FullSymbolName())),
            //        SeparatedList(
            //            new[]
            //            {
            //                VariableDeclarator(
            //                    Identifier(local.Name)
            //                )
            //            }
            //        )
            //    )
            //);

            // todo fix it
            return local.Access;
        }

        public override ExpressionSyntax VisitLiteral(ILiteralOperation operation, Scope _) => LiteralExpression(
            operation.Syntax.Kind(),
            operation.ConstantValue.Value switch
            {
                int value => Literal(value),
                string value => Literal(value),
                var a => throw new NotImplementedException()
            }
        );

        public override ExpressionSyntax VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope scope) =>
            AssignmentExpression(
                operation.Syntax.Kind(),
                operation.Target.Accept(this, scope),
                operation.Value.Accept(this, scope)
            );
    }
}