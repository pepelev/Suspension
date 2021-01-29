using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class OperationToExpression : OperationVisitor<Scope, ExpressionSyntax>
    {
        public override ExpressionSyntax DefaultVisit(IOperation operation, Scope argument) =>
            throw new NotSupportedException($"Operation {operation} not supported");

        public override ExpressionSyntax VisitInvocation(IInvocationOperation operation, Scope scope) =>
            SyntaxFactory.InvocationExpression(
                operation.Instance.Accept(this, scope),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        operation.Arguments.Select(
                            argument => SyntaxFactory.Argument(argument.Value.Accept(this, scope))
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

        public override ExpressionSyntax VisitLiteral(ILiteralOperation operation, Scope _) => SyntaxFactory.LiteralExpression(
            operation.Syntax.Kind(),
            operation.ConstantValue.Value switch
            {
                int value => SyntaxFactory.Literal(value),
                string value => SyntaxFactory.Literal(value),
                var a => throw new NotImplementedException()
            }
        );

        public override ExpressionSyntax VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope scope) =>
            SyntaxFactory.AssignmentExpression(
                operation.Syntax.Kind(),
                operation.Target.Accept(this, scope),
                operation.Value.Accept(this, scope)
            );
    }
}