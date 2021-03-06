﻿using System;
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
        public override ExpressionSyntax DefaultVisit(IOperation operation, Scope scope) =>
            throw operation.NotImplemented();

        public override ExpressionSyntax VisitSimpleAssignment(ISimpleAssignmentOperation operation, Scope scope) =>
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                operation.Target.Accept(this, scope),
                operation.Value.Accept(this, scope)
            );

        public override ExpressionSyntax VisitDeconstructionAssignment(
            IDeconstructionAssignmentOperation operation,
            Scope scope) =>
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                operation.Target.Accept(this, scope),
                operation.Value.Accept(this, scope)
            );

        public override ExpressionSyntax VisitTuple(ITupleOperation operation, Scope scope) => TupleExpression(
            SeparatedList(
                operation.Elements.Select(
                    element => Argument(element.Accept(this, scope))
                )
            )
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
                RefKind.None => Argument(NameColon(operation.Parameter.Name), Token(SyntaxKind.None), expression),
                RefKind.Out => Argument(NameColon(operation.Parameter.Name), Token(SyntaxKind.OutKeyword), expression),
                RefKind.Ref => Argument(NameColon(operation.Parameter.Name), Token(SyntaxKind.RefKeyword), expression),
                RefKind.In => Argument(NameColon(operation.Parameter.Name), Token(SyntaxKind.InKeyword), expression),
                _ => throw operation.NotImplemented()
            };
        }

        public override ExpressionSyntax VisitInterpolatedString(IInterpolatedStringOperation operation, Scope scope)
        {
            return InterpolatedStringExpression(
                Token(SyntaxKind.InterpolatedStringStartToken),
                List(
                    operation.Parts.Select(
                        part => part.Accept(new Interpolation(this), scope)
                    )
                )
            );
        }

        private sealed class Interpolation : OperationVisitor<Scope, InterpolatedStringContentSyntax>
        {
            private readonly OperationVisitor<Scope, ExpressionSyntax> expression;

            public Interpolation(OperationVisitor<Scope, ExpressionSyntax> expression)
            {
                this.expression = expression;
            }

            public override InterpolatedStringContentSyntax VisitInterpolatedStringText(
                IInterpolatedStringTextOperation operation,
                Scope scope)
            {
                var text = operation.Text.ConstantValue.ToString();
                return InterpolatedStringText(
                    Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, text, text, TriviaList())
                );
            }

            public override InterpolatedStringContentSyntax VisitInterpolation(
                IInterpolationOperation operation,
                Scope scope)
            {
                return (operation.FormatString, operation.Alignment) switch
                {
                    (null, null) => Interpolation(Syntax(operation.Expression)),
                    ({ } format, null) => Interpolation(
                        Syntax(operation.Expression),
                        formatClause: Format(format),
                        alignmentClause: null
                    ),
                    (null, { } alignment) => Interpolation(
                        Syntax(operation.Expression),
                        formatClause: null,
                        alignmentClause: Alignment(alignment)
                    ),
                    ({ } format, { } alignment) => Interpolation(
                        Syntax(operation.Expression),
                        formatClause: Format(format),
                        alignmentClause: Alignment(alignment)
                    )
                };

                ExpressionSyntax Syntax(IOperation subOperation) => subOperation.Accept(expression, scope);

                InterpolationAlignmentClauseSyntax Alignment(IOperation alignment) => InterpolationAlignmentClause(
                    Token(SyntaxKind.CommaToken),
                    Syntax(alignment)
                );

                static InterpolationFormatClauseSyntax Format(IOperation format)
                {
                    var text = format.ConstantValue.ToString();
                    return InterpolationFormatClause(
                        Token(SyntaxKind.ColonToken),
                        Token(
                            TriviaList(),
                            SyntaxKind.InterpolatedStringTextToken,
                            text,
                            text,
                            TriviaList()
                        )
                    );
                }
            }
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
        }

        public override ExpressionSyntax VisitArrayElementReference(
            IArrayElementReferenceOperation operation,
            Scope scope)
            => ElementAccessExpression(
                operation.ArrayReference.Accept(this, scope),
                BracketedArgumentList(
                    SeparatedList(
                        operation.Indices.Select(
                            index => Argument(index.Accept(this, scope))
                        )
                    )
                )
            );

        public override ExpressionSyntax VisitLiteral(ILiteralOperation operation, Scope _)
        {
            var (kind, token) = operation.ConstantValue.Value switch
            {
                int value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                uint value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                long value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                ulong value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                string value => (SyntaxKind.StringLiteralExpression, Literal(value)),
                char value => (SyntaxKind.CharacterLiteralExpression, Literal(value)),
                float value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                double value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                decimal value => (SyntaxKind.NumericLiteralExpression, Literal(value)),
                _ => throw operation.NotImplemented()
            };
            return LiteralExpression(kind, token);
        }

        public override ExpressionSyntax VisitCompoundAssignment(ICompoundAssignmentOperation operation, Scope scope) =>
            AssignmentExpression(
                operation.Syntax.Kind(),
                operation.Target.Accept(this, scope),
                operation.Value.Accept(this, scope)
            );

        public override ExpressionSyntax VisitUnaryOperator(IUnaryOperation operation, Scope scope) => PrefixUnaryExpression(
            operation.Syntax.Kind(),
            operation.Operand.Accept(this, scope)
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

        public override ExpressionSyntax VisitArrayCreation(IArrayCreationOperation operation, Scope scope)
        {
            var elementType = operation.Type.Accept(new ArrayElementType());
            var name = IdentifierName(elementType.Accept(FullSymbolName.WithGlobal));
            var rank = List(
                new[]
                {
                    ArrayRankSpecifier(
                        SeparatedList(
                            operation.DimensionSizes.Select(size => size.Accept(this, scope))
                        )
                    )
                }
            );
            var type = ArrayType(name, rank);

            if (operation.Initializer == null)
            {
                return ArrayCreationExpression(type);
            }

            return ArrayCreationExpression(
                type,
                Initializer(operation.Initializer, scope)
            );
        }

        public override ExpressionSyntax VisitArrayInitializer(IArrayInitializerOperation operation, Scope scope) =>
            Initializer(operation, scope);

        private InitializerExpressionSyntax Initializer(
            IArrayInitializerOperation operation,
            Scope scope)
            => InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                SeparatedList(
                    operation.ElementValues.Select(element => element.Accept(this, scope))
                )
            );

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

        public override ExpressionSyntax VisitDeclarationExpression(
            IDeclarationExpressionOperation operation,
            Scope scope) => operation.Expression.Accept(new DeclarationVisitor(this), scope);

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

        public override ExpressionSyntax VisitExpressionStatement(IExpressionStatementOperation operation, Scope scope) =>
            operation.Operation.Accept(this, scope);

        private sealed class DeclarationVisitor : OperationVisitor<Scope, ExpressionSyntax>
        {
            private readonly OperationVisitor<Scope, ExpressionSyntax> expression;

            public DeclarationVisitor(OperationVisitor<Scope, ExpressionSyntax> expression)
            {
                this.expression = expression;
            }

            public override ExpressionSyntax DefaultVisit(IOperation operation, Scope scope) =>
                throw operation.NotImplemented();

            public override ExpressionSyntax VisitLocalReference(ILocalReferenceOperation operation, Scope scope)
            {
                var value = new LocalValue(operation.Local);
                return scope.Find(value.Id).Access;
            }

            public override ExpressionSyntax VisitTuple(ITupleOperation operation, Scope scope) => TupleExpression(
                SeparatedList(
                    operation.Elements.Select(
                        element => Argument(element.Accept(expression, scope))
                    )
                )
            );
        }
    }
}