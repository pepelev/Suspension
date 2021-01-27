using System.Collections.Generic;
using System.Linq;
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

        public Dumb(string name, IMethodSymbol method, FlowPoint flowPoint, Scope start)
        {
            this.name = name;
            this.method = method;
            this.flowPoint = flowPoint;
            this.start = start;
        }

        public override SyntaxTree Document => CSharpSyntaxTree.Create(
            Namespace.NormalizeWhitespace(),
            path: $"{method.ContainingType.Accept(new FullSymbolName())}.Coroutines.{method.Name}.{name}.cs"
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
                    new[]
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
                        )
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
    }
}