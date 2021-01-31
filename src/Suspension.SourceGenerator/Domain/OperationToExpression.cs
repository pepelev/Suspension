using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain.Values;
using Suspension.SourceGenerator.Generator;
using Suspension.SourceGenerator.Predicates;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class OperationToExpression : OperationVisitor<Scope, ExpressionSyntax>
    {
        public override ExpressionSyntax DefaultVisit(IOperation operation, Scope argument) =>
            throw operation.NotImplemented();

        public override ExpressionSyntax VisitInvocation(IInvocationOperation operation, Scope scope) =>
            InvocationExpression(
                operation.Instance switch
                {
                    null => ParseName(operation.TargetMethod.Accept(new FullSymbolName())),
                    var instance => instance.Accept(this, scope)
                },
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

        public override ExpressionSyntax VisitBinaryOperator(IBinaryOperation operation, Scope scope) =>
            BinaryExpression(
                operation.Syntax.Kind(),
                operation.LeftOperand.Accept(this, scope),
                operation.RightOperand.Accept(this, scope)
            );

        public override ExpressionSyntax VisitIncrementOrDecrement(IIncrementOrDecrementOperation operation, Scope scope)
        {
            var target = operation.Target.Accept(this, scope);
            return (operation.IsPostfix, operation.Kind) switch
            {
                (true, OperationKind.Increment) => PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, target),
                (false, OperationKind.Increment) => PrefixUnaryExpression(SyntaxKind.PreIncrementExpression, target),
                (true, OperationKind.Decrement) => PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, target),
                (false, OperationKind.Decrement) => PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, target),
                _ => throw new InvalidOperationException()
            };
        }

        public override ExpressionSyntax VisitObjectCreation(IObjectCreationOperation operation, Scope scope)
        {
            if (operation.Initializer != null)
            {
                throw operation.Initializer.NotImplemented();
            }

            return ObjectCreationExpression(
                IdentifierName(operation.Type.Accept(new FullSymbolName())),
                ArgumentList(
                    SeparatedList(
                        operation.Arguments.Select(argument => Argument(argument.Value.Accept(this, scope)))
                    )
                ),
                null
            );
        }
    }
}