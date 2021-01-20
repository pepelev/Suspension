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

        public override SyntaxTree Document => CSharpSyntaxTree.Create(Namespace);

        private NamespaceDeclarationSyntax Namespace
        {
            get
            {
                return NamespaceDeclaration(
                    ParseName(method.Accept(new FullSymbolName())),
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
            Token(SyntaxTriviaList.Empty, SyntaxKind.IdentifierName, method.ContainingType.Name, method.ContainingType.Name, SyntaxTriviaList.Empty),
            TypeParameterList(),
            BaseList(),
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
            Token(SyntaxTriviaList.Empty, SyntaxKind.IdentifierName, "Coroutines", "Coroutines", SyntaxTriviaList.Empty),
            TypeParameterList(),
            BaseList(),
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
            Token(SyntaxTriviaList.Empty, SyntaxKind.IdentifierName, method.Name, method.Name, SyntaxTriviaList.Empty),
            TypeParameterList(),
            BaseList(),
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
            Token(SyntaxTriviaList.Empty, SyntaxKind.IdentifierName, name, name, SyntaxTriviaList.Empty),
            TypeParameterList(),
            BaseList(),
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
                        Token(SyntaxTriviaList.Empty, SyntaxKind.IdentifierName, "Completed", "Completed", SyntaxTriviaList.Empty),
                        null,
                        ArrowExpressionClause(
                            LiteralExpression(SyntaxKind.FalseKeyword)
                        ),
                        null
                    ),
                    PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList(
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        ),
                        ParseTypeName("Suspension.None"),
                        null,
                        Token(SyntaxTriviaList.Empty, SyntaxKind.IdentifierName, "Result", "Result", SyntaxTriviaList.Empty),
                        null,
                        ArrowExpressionClause(
                            ThrowExpression(
                                ObjectCreationExpression(
                                    ParseTypeName("System.InvalidOperationException")
                                )
                            )
                        ),
                        null
                    )
                }
            )
        );
    }
}