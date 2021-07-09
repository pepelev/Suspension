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
            path: $"{method.ContainingType.Accept(new NoGlobalFullSymbolName())}.Coroutines.{method.Name}.{Name}.cs",
            encoding: Encoding.UTF8
        );

        private NamespaceDeclarationSyntax Namespace => NamespaceDeclaration(
            ParseName(method.ContainingType.ContainingNamespace.Accept(new FullSymbolName())),
            List<ExternAliasDirectiveSyntax>(),
            List<UsingDirectiveSyntax>(),
            List<MemberDeclarationSyntax>(
                new[] { OriginalClass }
            )
        );

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
                new[] { CoroutinesClass }
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
                new[] { MethodClass }
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
                            ParseTypeName(
                                $"{method.ContainingType.Accept(new FullSymbolName())}.Coroutines.{method.Name}"
                            )
                        )
                    }
                )
            ),
            List<TypeParameterConstraintClauseSyntax>(),
            List(
                new MemberDeclarationSyntax[] {Completed, Run, Accept}
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
                LiteralExpression(SyntaxKind.TrueLiteralExpression)
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
        );

        private static MethodDeclarationSyntax Accept => MethodDeclaration(
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
                    new[] {TypeParameter("T")}
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
                        IdentifierName("VisitExit")
                    )
                )
            ),
            Token(SyntaxKind.SemicolonToken)
        );
    }
}