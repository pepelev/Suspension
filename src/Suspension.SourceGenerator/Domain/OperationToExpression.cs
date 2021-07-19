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

        public override ExpressionSyntax VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope scope) =>
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                operation.Target.Accept(this, scope),
                operation.Value.Accept(this, scope)
            );

        public override ExpressionSyntax VisitInvocation(IInvocationOperation operation, Scope scope) =>
            InvocationExpression(
                operation.Instance switch
                {
                    null => ParseName(operation.TargetMethod.Accept(FullSymbolName.WithGlobal)),
                    var instance => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        instance.Accept(this, scope),
                        IdentifierName(operation.TargetMethod.Name)
                    )
                },
                ArgumentList(
                    SeparatedList(
                        operation.Arguments.Select(
                            argument => VisitArgument(argument, scope)
                        )
                    )
                )
            );

        private new ArgumentSyntax VisitArgument(IArgumentOperation operation, Scope scope)
        {
            var expression = operation.Value.Accept(this, scope);
            return operation.Parameter.RefKind switch
            {
                RefKind.None => Argument(expression),
                RefKind.Out => Argument(null, Token(SyntaxKind.OutKeyword), expression),
                _ => throw operation.NotImplemented()
            };
        }

        public override ExpressionSyntax VisitParameterReference(IParameterReferenceOperation operation, Scope scope)
        {
            var parameter = new ParameterValue(operation.Parameter);
            return scope.Find(parameter.Id).Access;
        }

        public override ExpressionSyntax VisitLocalReference(ILocalReferenceOperation operation, Scope scope)
        {
            var local = new LocalValue(operation.Local);
            return scope.Find(local.Id).Access;

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
                char value => Literal(value),
                _ => throw new NotImplementedException()
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
                IdentifierName(operation.Type.Accept(FullSymbolName.WithGlobal)),
                ArgumentList(
                    SeparatedList(
                        operation.Arguments.Select(argument => VisitArgument(argument, scope))
                    )
                ),
                null
            );
        }

        public override ExpressionSyntax VisitConversion(IConversionOperation operation, Scope currentScope)
        {
            var conversion = operation.GetConversion();
            var expression = operation.Operand.Accept(this, currentScope);
            return conversion.IsImplicit
                ? expression
                : CastExpression(
                    IdentifierName(operation.Type.Accept(FullSymbolName.WithGlobal)),
                    expression
                );
        }

        public override ExpressionSyntax VisitDiscardOperation(IDiscardOperation operation, Scope scope) =>
            IdentifierName("_");

        public override ExpressionSyntax VisitDeclarationExpression(IDeclarationExpressionOperation operation, Scope scope)
        {
            var local = operation.Expression.Accept(new LocalVisitor());
            var value = new LocalValue(local);
            return scope.Find(value.Id).Access;
        }

        public override ExpressionSyntax VisitFieldReference(IFieldReferenceOperation operation, Scope scope) =>
            MemberReference(operation, scope);

        public override ExpressionSyntax VisitPropertyReference(IPropertyReferenceOperation operation, Scope scope) =>
            MemberReference(operation, scope);

        private ExpressionSyntax MemberReference(IMemberReferenceOperation operation, Scope scope)
        {
            var expression = operation.Instance switch
            {
                null => IdentifierName(operation.Member.ContainingType.Accept(FullSymbolName.WithGlobal)),
                { } instance => instance.Accept(this, scope)
            };

            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                expression,
                IdentifierName(operation.Member.Name)
            );
        }

        private sealed class LocalVisitor : OperationVisitor<None, ILocalSymbol>
        {
            public override ILocalSymbol DefaultVisit(IOperation operation, None argument) =>
                throw operation.NotImplemented();

            public override ILocalSymbol VisitLocalReference(ILocalReferenceOperation operation, None argument) =>
                operation.Local;
        }
    }
}