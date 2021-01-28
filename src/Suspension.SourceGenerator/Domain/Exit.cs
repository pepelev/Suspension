using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Suspension.SourceGenerator.Predicates;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class Exit : Coroutine
    {
        private readonly IMethodSymbol method;

        public Exit(IMethodSymbol method)
        {
            this.method = method;
            if (!method.ReturnsVoid)
                throw new NotImplementedException("Only void method are implemented so far");
        }

        private string Name => "Exit";

        public override SyntaxTree Document => CSharpSyntaxTree.Create(
            Namespace.NormalizeWhitespace(),
            path: $"{method.ContainingType.Accept(new FullSymbolName())}.Coroutines.{method.Name}.{Name}.cs",
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
                        new[] { OriginalClass }
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
                new[] { CoroutinesClass }
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
                new[] { MethodClass }
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
                new[] { CoroutineClass }
            )
        );



        private ClassDeclarationSyntax CoroutineClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.SealedKeyword)
            ),
            Identifier(Name),
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
                            LiteralExpression(SyntaxKind.TrueLiteralExpression)
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
                            ObjectCreationExpression(
                                ParseTypeName("Suspension.None"),
                                ArgumentList(),
                                null
                            )
                        ),
                        null,
                        Token(SyntaxKind.SemicolonToken)
                    ),
                    MethodDeclaration(
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
                        Token(SyntaxKind.SemicolonToken)
                    )
                }
            )
        );
    }
}