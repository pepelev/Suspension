using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Predicates;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class Base : Coroutine
    {
        private readonly Graph3 graph;
        private readonly IMethodSymbol method;

        public Base(IMethodSymbol method, Graph3 graph)
        {
            this.graph = graph;
            this.method = method;
        }

        public override SyntaxTree Document => CSharpSyntaxTree.Create(
            Namespace.NormalizeWhitespace(),
            path: $"{method.ContainingType.Accept(new NoGlobalFullSymbolName())}.Coroutines.{method.Name}.cs",
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
            List(
                new MemberDeclarationSyntax[] { VisitMethod, Visitor }
            )
        );

        private MethodDeclarationSyntax VisitMethod => MethodDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.AbstractKeyword)
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
            null,
            Token(SyntaxKind.SemicolonToken)
        );

        public ClassDeclarationSyntax Visitor => ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.AbstractKeyword)
            ),
            Identifier("Visitor"),
            TypeParameterList(
                SeparatedList(
                    new[] { TypeParameter("T") }
                )
            ),
            null,
            List<TypeParameterConstraintClauseSyntax>(),
            List<MemberDeclarationSyntax>(
                graph.Select(
                    pair => MethodDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList(
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.AbstractKeyword)
                        ),
                        ParseTypeName("T"),
                        null,
                        Identifier($"Visit{pair.Suspension}"),
                        null,
                        ParameterList(
                            SeparatedList(
                                pair.References.Select(
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
                        List<TypeParameterConstraintClauseSyntax>(),
                        null,
                        null,
                        Token(SyntaxKind.SemicolonToken)
                    )
                )
            )
        );
    }
}