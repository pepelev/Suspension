using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain.Values;
using Suspension.SourceGenerator.Generator;
using Suspension.SourceGenerator.Predicates;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class Dumb : Coroutine
    {
        private readonly string name;
        private readonly IMethodSymbol method;
        private readonly FlowPoint flowPoint;
        private readonly Scope start;
        private readonly Graph3 graph;

        public Dumb(string name, IMethodSymbol method, FlowPoint flowPoint, Scope start, Graph3 graph)
        {
            this.name = name;
            this.method = method;
            this.flowPoint = flowPoint;
            this.start = start;
            this.graph = graph;
        }

        public override SyntaxTree Document => CSharpSyntaxTree.Create(
            Namespace.NormalizeWhitespace(),
            path: $"{method.ContainingType.Accept(new NoGlobalFullSymbolName())}.Coroutines.{method.Name}.{name}.cs",
            encoding: Encoding.UTF8
        );

        private NamespaceDeclarationSyntax Namespace
        {
            get
            {
                return NamespaceDeclaration(
                    ParseName(method.ContainingType.ContainingNamespace.Accept(new FullSymbolName())),
                    List<ExternAliasDirectiveSyntax>(),
                    List<UsingDirectiveSyntax>(),
                    List<MemberDeclarationSyntax>(
                        new[] {OriginalClass}
                    )
                ).WithLeadingTrivia(
                    Trivia(
                        PragmaWarningDirectiveTrivia(
                            Token(SyntaxKind.DisableKeyword),
                            SeparatedList<ExpressionSyntax>(
                                new[]
                                {
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(Warnings.UnreachableCode)
                                    ),
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(Warnings.LabelNotReferenced)
                                    )
                                }
                            ),
                            true
                        )
                    )
                );
            }
        }

        private ClassDeclarationSyntax OriginalClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PartialKeyword)
            ),
            Identifier(method.ContainingType.Name),
            typeParameterList: null,
            baseList: null,
            List<TypeParameterConstraintClauseSyntax>(),
            List<MemberDeclarationSyntax>(
                new[] {CoroutinesClass}
            )
        );

        private ClassDeclarationSyntax CoroutinesClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PartialKeyword)
            ),
            Identifier("Coroutines"),
            typeParameterList: null,
            baseList: null,
            List<TypeParameterConstraintClauseSyntax>(),
            List<MemberDeclarationSyntax>(
                new[] {MethodClass}
            )
        );

        private ClassDeclarationSyntax MethodClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PartialKeyword)
            ),
            Identifier(method.Name),
            typeParameterList: null,
            null,
            List<TypeParameterConstraintClauseSyntax>(),
            List<MemberDeclarationSyntax>(
                new[] {CoroutineClass}
            )
        );

        private ClassDeclarationSyntax CoroutineClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword)
            ),
            Identifier(name),
            typeParameterList: null,
            BaseList(
                SeparatedList<BaseTypeSyntax>(
                    new[]
                    {
                        SimpleBaseType(
                            ParseTypeName(method.ContainingType.Accept(new FullSymbolName()) + ".Coroutines." + method.Name)
                        )
                    }
                )
            ),
            List<TypeParameterConstraintClauseSyntax>(),
            List(
                Fields.Concat(
                    new MemberDeclarationSyntax[] {Constructor, Completed, Run, Accept}
                )
            )
        );

        private MethodDeclarationSyntax Accept => MethodDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            ),
            ParseTypeName("T"),
            null,
            Identifier("Accept"),
            TypeParameterList(
                SeparatedList(
                    new[] { TypeParameter("T") }
                )
            ),
            ParameterList(
                SeparatedList(
                    new[]
                    {
                        Parameter(
                            List<AttributeListSyntax>(),
                            TokenList(),
                            ParseTypeName("Visitor<T>"),
                            Identifier("visitor"),
                            null
                        )
                    }
                )
            ),
            List<TypeParameterConstraintClauseSyntax>(),
            null,
            ArrowExpressionClause(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("visitor"),
                        IdentifierName($"Visit{name}")
                    ),
                    ArgumentList(
                        SeparatedList(
                            start.Select(
                                value => Argument(value.Access)
                            )
                        )
                    )
                )
            ),
            Token(SyntaxKind.SemicolonToken)
        );


        private IEnumerable<MemberDeclarationSyntax> Fields => start.Select(
            value => FieldDeclaration(
                List<AttributeListSyntax>(),
                TokenList(
                    Token(SyntaxKind.PrivateKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword)
                ),
                VariableDeclaration(
                    ParseTypeName(value.Type.Accept(new FullSymbolName())),
                    SeparatedList(
                        new[]
                        {
                            VariableDeclarator(value.Name)
                        }
                    )
                )
            )
        );

        private ConstructorDeclarationSyntax Constructor => ConstructorDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword)
            ),
            Identifier(name),
            ParameterList(
                SeparatedList(
                    start.Select(
                        value => Parameter(
                            List<AttributeListSyntax>(),
                            TokenList(),
                            ParseTypeName(value.Type.Accept(new FullSymbolName())),
                            Identifier(value.Name),
                            null
                        )
                    )
                )
            ),
            null,
            Block(
                List<StatementSyntax>(
                    start.Select(
                        value => ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(value.Name)
                                ),
                                IdentifierName(value.Name)
                            )
                        )
                    )
                )
            )
        );

        private static PropertyDeclarationSyntax Completed => PropertyDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            ),
            ParseTypeName("System.Boolean"),
            null,
            Identifier("Completed"),
            null,
            ArrowExpressionClause(
                LiteralExpression(SyntaxKind.FalseLiteralExpression)
            ),
            null,
            Token(SyntaxKind.SemicolonToken)
        );

        private MethodDeclarationSyntax Run => MethodDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            ),
            ParseTypeName($"{method.ContainingType.Accept(new FullSymbolName())}.Coroutines.{method.Name}"),
            null,
            Identifier("Run"),
            null,
            ParameterList(),
            List<TypeParameterConstraintClauseSyntax>(),
            RunBody,
            null
        );

        private BlockSyntax RunBody => Block(
            List(Statements)
        );

        private IEnumerable<StatementSyntax> Statements
        {
            get
            {
                static SyntaxToken Label(FlowPoint point)
                {
                    var ordinal = point.Block.Ordinal.ToString(CultureInfo.InvariantCulture);
                    var index = point.Index.ToString(CultureInfo.InvariantCulture);
                    return Identifier($"block{ordinal}_{index}");
                }

                var startPoint = flowPoint;
                var scope = graph.Single(pair => pair.Suspension == name).References;

                foreach (var value in scope)
                {
                    yield return LocalDeclarationStatement(
                        List<AttributeListSyntax>(),
                        TokenList(),
                        VariableDeclaration(
                            IdentifierName(value.Type.Accept(new FullSymbolName())),
                            SeparatedList(
                                new[]
                                {
                                    VariableDeclarator(
                                        Identifier($"{value.Name}Variable"),
                                        null,
                                        EqualsValueClause(
                                            value.Access
                                        )
                                    )
                                }
                            )
                        )
                    );
                }

                scope = new ConstantScope(
                    scope.Select(value => new RedirectedToLocal(value, "Variable"))
                );

                var visited = new HashSet<BasicBlock>();
                var queue = new Queue<FlowPoint>();
                queue.Enqueue(startPoint);

                while (queue.Count > 0)
                {
                    var point = queue.Dequeue();
                    var block = point.Block;
                    if (visited.Contains(block))
                        continue;

                    foreach (var local in block.EnclosingRegion.Locals.Select(local => new LocalValue(local)).Except(scope))
                    {
                        yield return LocalDeclarationStatement(
                            List<AttributeListSyntax>(),
                            TokenList(),
                            VariableDeclaration(
                                IdentifierName(local.Type.Accept(new FullSymbolName())),
                                SeparatedList(
                                    new[]
                                    {
                                        VariableDeclarator(
                                            Identifier(local.Name),
                                            null,
                                            EqualsValueClause(
                                                DefaultExpression(ParseTypeName(local.Type.Accept(new FullSymbolName()))) // todo may be there is elegant solution
                                            )
                                        )
                                    }
                                )
                            )
                        );
                    }

                    yield return GotoStatement(
                        SyntaxKind.GotoStatement,
                        IdentifierName(
                            Label(point)
                        )
                    );
                    yield return LabeledStatement(
                        Label(new FlowPoint(block)),
                        EmptyStatement()
                    );

                    if (block.Kind == BasicBlockKind.Exit)
                    {
                        var className = $"{method.ContainingSymbol.Accept(new FullSymbolName())}.Coroutines.{method.Name}.Exit";
                        yield return ReturnStatement(
                            ObjectCreationExpression(
                                ParseTypeName(className),
                                ArgumentList(),
                                null
                            )
                        ).WithLeadingTrivia(
                            Trivia(
                                LineDirectiveTrivia(
                                    Token(SyntaxKind.DefaultKeyword),
                                    true
                                )
                            )
                        );
                    }

                    for (var i = 0; i < block.Operations.Length; i++)
                    {
                        var operation = block.Operations[i];
                        if (operation.Accept(new SuspensionPoint.Is()))
                        {
                            var suspensionPointName = operation.Accept(new SuspensionPoint.Name());
                            var targetScope = graph.Single(pair => pair.Suspension == suspensionPointName).References;
                            var className = $"{method.ContainingSymbol.Accept(new FullSymbolName())}.Coroutines.{method.Name}.{suspensionPointName}";
                            var currentScope = scope;
                            yield return ReturnStatement(
                                ObjectCreationExpression(
                                    ParseTypeName(className),
                                    ArgumentList(
                                        SeparatedList(
                                            targetScope.Select(value => Argument(currentScope.Find(value).Access))
                                        )
                                    ),
                                    null
                                )
                            ).WithLeadingTrivia(
                                Trivia(
                                    LineDirectiveTrivia(
                                        Token(SyntaxKind.DefaultKeyword),
                                        true
                                    )
                                )
                            );

                            yield return LabeledStatement(
                                Label(new FlowPoint(block, i + 1)),
                                EmptyStatement()
                            );
                        }
                        else
                        {
                            var statement = operation.Accept(new OperationToStatement(), scope);
                            scope = operation.Accept(new ScopeDeclaration(), scope);
                            yield return statement.WithLeadingTrivia(
                                Trivia(
                                    LineDirectiveTrivia(
                                        Literal(operation.Syntax.SyntaxTree.GetLineSpan(operation.Syntax.Span).StartLinePosition.Line + 1),
                                        Literal(operation.Syntax.SyntaxTree.FilePath),
                                        true
                                    )
                                )
                            );
                        }
                    }

                    if (block.ConditionalSuccessor is { } conditional)
                    {
                        var expression = block.BranchValue.Accept(new OperationToExpression(), scope);
                        var destination = new FlowPoint(conditional.Destination);
                        yield return IfStatement(
                            block.ConditionKind switch
                            {
                                ControlFlowConditionKind.WhenTrue => expression,
                                ControlFlowConditionKind.WhenFalse => PrefixUnaryExpression(
                                    SyntaxKind.LogicalNotExpression,
                                    ParenthesizedExpression(expression)
                                ),
                                _ => throw new InvalidOperationException()
                            },
                            GotoStatement(
                                SyntaxKind.GotoStatement,
                                IdentifierName(
                                    Label(destination)
                                )
                            )
                        ).WithLeadingTrivia(
                            Trivia(
                                LineDirectiveTrivia(
                                    Literal(block.BranchValue.Syntax.SyntaxTree.GetLineSpan(block.BranchValue.Syntax.Span).StartLinePosition.Line + 1),
                                    Literal(block.BranchValue.Syntax.SyntaxTree.FilePath),
                                    true
                                )
                            )
                        );

                        queue.Enqueue(destination);
                    }

                    if (block.FallThroughSuccessor is { } fallThrough)
                    {
                        if (fallThrough.Semantics == ControlFlowBranchSemantics.Regular)
                        {
                            var destination = new FlowPoint(fallThrough.Destination);
                            yield return GotoStatement(
                                SyntaxKind.GotoStatement,
                                IdentifierName(
                                    Label(destination)
                                )
                            );

                            queue.Enqueue(destination);
                        }
                        else if (fallThrough.Semantics == ControlFlowBranchSemantics.Throw)
                        {
                            yield return ThrowStatement(
                                block.BranchValue.Accept(new OperationToExpression(), scope)
                            ).WithLeadingTrivia(
                                Trivia(
                                    LineDirectiveTrivia(
                                        Literal(block.BranchValue.Syntax.SyntaxTree.GetLineSpan(block.BranchValue.Syntax.Span).StartLinePosition.Line + 1),
                                        Literal(block.BranchValue.Syntax.SyntaxTree.FilePath),
                                        true
                                    )
                                )
                            );
                        }
                    }

                    visited.Add(block);
                }
            }
        }
    }
}