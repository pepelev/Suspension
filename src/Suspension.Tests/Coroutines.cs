using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Suspension.Tests
{
    public sealed class Coroutines : IEnumerable<SyntaxTree>
    {
        private readonly CSharpCompilation compilation;

        public Coroutines(CSharpCompilation compilation)
        {
            this.compilation = compilation;
        }

        public IEnumerator<SyntaxTree> GetEnumerator()
        {
            var emitResult = compilation.Emit(Stream.Null);
            if (!emitResult.Success)
            {
                throw new Exception($"Compilation failed with {string.Join("; ", emitResult.Diagnostics)}");
            }

            return (
                from tree in compilation.SyntaxTrees
                from csharpSyntaxTree in Trees(tree)
                select csharpSyntaxTree
            ).GetEnumerator();
        }

        private IEnumerable<SyntaxTree> Trees(SyntaxTree tree)
        {
            var semantic = compilation.GetSemanticModel(tree);
            var syntaxNodes = tree.GetRoot().DescendantNodes().ToList();
            var methodDeclaration = syntaxNodes
                .OfType<MethodDeclarationSyntax>()
                .Where(
                    method => semantic
                        .GetDeclaredSymbol(method)
                        .GetAttributes()
                        .Any(attribute =>
                            {
                                var @class = attribute.AttributeClass;
                                return @class.Name == "SuspendableAttribute" &&
                                       @class.ContainingNamespace.ToString() == "Suspension";
                            }
                        )
                );

            foreach (var syntax in methodDeclaration)
            {
                var symbol = semantic.GetDeclaredSymbol(syntax) ?? throw new Exception("GetDeclaredSymbol failed");

                var graph = ControlFlowGraph.Create(syntax, semantic);

                var entry = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Entry);
                var exit = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Exit);

                var parameters = new Playground.MethodParameters();
                var d = (
                    from block in graph.Blocks.Except(new[] {entry, exit})
                    from operation in block.Operations
                    from parameter in operation.Accept(parameters, new None())
                    select parameter
                ).ToList();
                var instructions = new V2();
                var operations = (
                    from block in graph.Blocks.Except(new[] {entry, exit})
                    from operation in block.Operations
                    select operation.Accept(instructions, new None())
                ).ToList();
                var fields = string.Join(
                    "\n",
                    d.Select(pair => $"private readonly {pair.Type} {pair.Name};")
                );
                var arguments = string.Join(
                    ", ",
                    d.Select(pair => $"{pair.Type} {pair.Name}")
                );
                var assignments = string.Join(
                    "\n",
                    d.Select(pair => $"this.{pair.Name} = {pair.Name};")
                );
                var textStart = $@"namespace {symbol.ContainingNamespace}
{{
    public static partial class {symbol.ContainingType.Name}
    {{
        public static partial class Coroutines
        {{
            public static partial class {symbol.Name}
            {{
                public sealed class Start : Coroutine<None>
                {{
{fields}
                    public Start({arguments})
                    {{
{assignments}
                    }}

                    public override bool Completed => false;
                    public override None Result => throw new System.InvalidOperationException();

                    public override Coroutine<None> Run()
                    {{
{string.Join("\n", operations)}
                        return new Finish();
                    }}
                }}
            }}
        }}
    }}
}}";

                var textFinish = $@"namespace {symbol.ContainingNamespace}
{{
    public static partial class {symbol.ContainingType.Name}
    {{
        public static partial class Coroutines
        {{
            public static partial class {symbol.Name}
            {{
                public sealed class Finish : Coroutine<None>
                {{
                    public override bool Completed => true;
                    public override None Result => new None();

                    public override Coroutine<None> Run()
                    {{
                        throw new System.InvalidOperationException();
                    }}
                }}
            }}
        }}
    }}
}}";
                yield return CSharpSyntaxTree.ParseText(
                    textStart,
                    path: $"{symbol.ContainingType.Name}.Coroutines.{symbol.Name}.Start.cs"
                );
                yield return CSharpSyntaxTree.ParseText(
                    textFinish,
                    path: $"{symbol.ContainingType.Name}.Coroutines.{symbol.Name}.Finish.cs"
                );
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class V2 : OperationVisitor<None, string>
    {
        public override string DefaultVisit(IOperation operation, None argument)
        {
            throw new Exception($"V2 Visit failed {operation}"); ;
        }

        public override string VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
        {
            return operation.Operation.Accept(this, argument);
        }

        public override string VisitInvocation(IInvocationOperation operation, None none)
        {
            var instance = operation.Instance.Accept(this, none);
            var arguments = string.Join(
                ", ",
                operation.Arguments.Select(argument => argument.Accept(this, none))
            );

            return $"{instance}({arguments});";
        }

        public override string VisitParameterReference(IParameterReferenceOperation operation, None argument)
            => $"this.{operation.Parameter.Name}";
    }
}