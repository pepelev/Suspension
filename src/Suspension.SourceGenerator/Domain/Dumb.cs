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
        private readonly int operationIndex;
        private readonly BasicBlock block;

        public Dumb(string name, IMethodSymbol method, int operationIndex, BasicBlock block)
        {
            this.name = name;
            this.operationIndex = operationIndex;
            this.block = block;
            this.method = method;
        }

        public override SyntaxTree Document => CSharpSyntaxTree.Create(Namespace.NormalizeWhitespace());

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
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.PartialKeyword)
            ),
            Identifier(method.Name),
            typeParameterList: null,
            baseList: null,
            List<TypeParameterConstraintClauseSyntax>(),
            List<MemberDeclarationSyntax>(
                new[] {CoroutineClass}
            )
        );

        private ClassDeclarationSyntax CoroutineClass => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.PartialKeyword)
            ),
            Identifier(name),
            typeParameterList: null,
            baseList: null,
            List<TypeParameterConstraintClauseSyntax>(),
            List<MemberDeclarationSyntax>(
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
        );
    }
}