using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
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
            path: $"{method.ContainingType.Accept(new FullSymbolName())}.Coroutines.{method.Name}.{name}.cs",
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
                );
            }
        }

        private ClassDeclarationSyntax OriginalClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
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
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
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
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.AbstractKeyword),
                Token(SyntaxKind.PartialKeyword)
            ),
            Identifier(method.Name),
            typeParameterList: null,
            BaseList(
                SeparatedList<BaseTypeSyntax>(
                    new[]
                    {
                        SimpleBaseType(
                            GenericName(
                                Identifier("Suspension.Coroutine"),
                                TypeArgumentList(
                                    SeparatedList(
                                        new[] { ParseTypeName("Suspension.None") }
                                    )
                                )
                            )
                        )
                    }
                )
            ),
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
                Payload.Append(Constructor).Concat(
                    new MemberDeclarationSyntax[]
                    {
                        PropertyDeclaration(
                            List<AttributeListSyntax>(),
                            TokenList(
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.OverrideKeyword)
                            ),
                            ParseTypeName("System.Boolean"),
                            null,
                            Identifier(nameof(Coroutine<None>.Completed)),
                            null,
                            ArrowExpressionClause(
                                LiteralExpression(SyntaxKind.FalseLiteralExpression)
                            ),
                            null,
                            Token(SyntaxKind.SemicolonToken)
                        ),
                        PropertyDeclaration(
                            List<AttributeListSyntax>(),
                            TokenList(
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.OverrideKeyword)
                            ),
                            ParseTypeName("Suspension.None"),
                            null,
                            Identifier(nameof(Coroutine<None>.Result)),
                            null,
                            ArrowExpressionClause(
                                ThrowExpression(
                                    ObjectCreationExpression(
                                        ParseTypeName("System.InvalidOperationException"),
                                        ArgumentList(),
                                        null
                                    )
                                )
                            ),
                            null,
                            Token(SyntaxKind.SemicolonToken)
                        ),
                        Run
                    }
                )
            )
        );

        private IEnumerable<MemberDeclarationSyntax> Payload => start.Select(
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

        private MethodDeclarationSyntax Run => MethodDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            ),
            ParseTypeName("Suspension.Coroutine<Suspension.None>"),
            null,
            Identifier(nameof(Coroutine<None>.Run)),
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
                static SyntaxToken Label(BasicBlock block)
                {
                    return Identifier($"block{block.Ordinal.ToString(CultureInfo.InvariantCulture)}");
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

                    yield return LabeledStatement(
                        Label(block),
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
                        );
                    }

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
                                            Identifier(local.Name)
                                        )
                                    }
                                )
                            )
                        );
                    }

                    for (var i = point.Index; i < block.Operations.Length; i++)
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
                            );
                            goto m1;
                        }

                        var statement = operation.Accept(new OperationToStatement(), scope);
                        scope = operation.Accept(new ScopeDeclaration(), scope);
                        yield return statement;
                    }

                    if (block.ConditionalSuccessor is { } conditional)
                    {
                        var expression = block.BranchValue.Accept(new OperationToExpression(), scope);
                        yield return IfStatement(
                            block.ConditionKind switch
                            {
                                ControlFlowConditionKind.WhenTrue => expression,
                                ControlFlowConditionKind.WhenFalse => PrefixUnaryExpression(
                                    SyntaxKind.LogicalNotExpression,
                                    expression
                                ),
                                _ => throw new InvalidOperationException()
                            },
                            GotoStatement(
                                SyntaxKind.GotoStatement,
                                IdentifierName(
                                    Label(conditional.Destination)
                                )
                            )
                        );

                        queue.Enqueue(new FlowPoint(conditional.Destination, 0));
                    }

                    if (block.FallThroughSuccessor is { } fallThrough)
                    {
                        yield return GotoStatement(
                            SyntaxKind.GotoStatement,
                            IdentifierName(
                                Label(fallThrough.Destination)
                            )
                        );

                        queue.Enqueue(new FlowPoint(fallThrough.Destination, 0));
                    }

                    m1: visited.Add(block);
                }
            }
        }
    }
}