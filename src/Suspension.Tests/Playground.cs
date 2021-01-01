using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Framework;

namespace Suspension.Tests
{
    public class Playground
    {
        [Test]
        public void Test()
        {
            var tree = CSharpSyntaxTree.ParseText(
                @"using System;

namespace Playground.Test
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SuspendAttribute : Attribute {}

    public sealed class Example
    {
        [Suspend]
        private static void Suspend()
        {
        }

        public void Run(Action<int> action) // start
        {
            action(15);
        } // finish
    }
}"
            );

            var compilation = CSharpCompilation.Create(
                "Playground.Test",
                new[] {tree},
                new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)},
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var emitResult = compilation.Emit(Stream.Null);
            var semantic = compilation.GetSemanticModel(tree);
            var syntaxNodes = tree.GetRoot().DescendantNodes().ToList();
            var syntax = syntaxNodes
                .OfType<MethodDeclarationSyntax>()
                .Single(method => semantic.GetDeclaredSymbol(method)?.Name == "Run");

            var symbol = semantic.GetDeclaredSymbol(syntax) ?? throw new Exception("GetDeclaredSymbol failed");

            var graph = ControlFlowGraph.Create(syntax, semantic);

            var entry = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Entry);
            var exit = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Exit);

            var parameters = new MethodParameters();
            var d = (
                from block in graph.Blocks.Except(new[] {entry, exit})
                from operation in block.Operations
                from parameter in operation.Accept(parameters, new None())
                select parameter
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

            Console.WriteLine(
                $@"namespace {symbol.ContainingNamespace}
{{
    public sealed class Start : Suspension.Coroutine<None>
    {{
{fields}
        public Start({arguments})
        {{
{assignments}
        }}
        public override bool Completed => false;
        public override None Result => throw new InvalidOperationException();

        public override Coroutine<None> Run()
        {{
            action(15);
            return new Finish();
        }}
    }}

    public sealed class Finish : Coroutine<None>
    {{
        public override bool Completed => true;
        public override None Result => new None();
        public override Coroutine<None> Run() => throw new InvalidOperationException();
    }}
}}"
            );
        }

        private sealed class MethodParameters : OperationVisitor<None, IEnumerable<(string Type, string Name)>>
        {
            public override IEnumerable<(string Type, string Name)> DefaultVisit(IOperation operation, None argument)
            {
                throw new Exception($"V Visit failed {operation}");
            }

            public override IEnumerable<(string Type, string Name)> VisitInvocation(IInvocationOperation operation, None argument) =>
                from child in operation.Arguments.Append(operation.Instance)
                from result in child.Accept(this, argument)
                select result;

            public override IEnumerable<(string Type, string Name)> VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
            {
                return operation.Operation.Accept(this, argument);
            }

            public override IEnumerable<(string Type, string Name)> VisitArgument(IArgumentOperation operation, None argument)
            {
                return operation.Value.Accept(this, argument);
            }

            public override IEnumerable<(string Type, string Name)> VisitLiteral(ILiteralOperation operation, None argument)
            {
                return Array.Empty<(string Type, string Name)>();
            }

            public override IEnumerable<(string Type, string Name)> VisitParameterReference(IParameterReferenceOperation operation, None argument)
            {
                var parameter = operation.Parameter;
                return new[]
                {
                    (parameter.Type.ToString(), parameter.Name)
                };
            }

            public override IEnumerable<(string Type, string Name)> VisitSimpleAssignment(ISimpleAssignmentOperation operation, None argument)
            {
                return Array.Empty<(string Type, string Name)>();
            }

            public override IEnumerable<(string Type, string Name)> VisitLocalReference(ILocalReferenceOperation operation, None argument)
            {
                return Array.Empty<(string Type, string Name)>();
            }
        }

        private sealed class V : OperationVisitor<None, IEnumerable<string>>
        {
            public override IEnumerable<string> DefaultVisit(IOperation operation, None argument)
            {
                throw new Exception($"V Visit failed {operation}");
            }

            public override IEnumerable<string> VisitInvocation(IInvocationOperation operation, None argument) =>
                from child in operation.Arguments.Append(operation.Instance)
                from result in child.Accept(this, argument)
                select result;

            public override IEnumerable<string> VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
            {
                return operation.Operation.Accept(this, argument);
            }

            public override IEnumerable<string> VisitArgument(IArgumentOperation operation, None argument)
            {
                return operation.Value.Accept(this, argument);
            }

            public override IEnumerable<string> VisitLiteral(ILiteralOperation operation, None argument)
            {
                return Array.Empty<string>();
            }

            public override IEnumerable<string> VisitParameterReference(IParameterReferenceOperation operation, None argument)
            {
                var parameter = operation.Parameter;
                return new[] {$"parameter: {parameter.Type} {parameter.Name}"};
            }

            public override IEnumerable<string> VisitSimpleAssignment(ISimpleAssignmentOperation operation, None argument)
            {
                return Array.Empty<string>();
            }

            public override IEnumerable<string> VisitLocalReference(ILocalReferenceOperation operation, None argument)
            {
                var local = operation.Local;
                return new[] {$"local: {local.Type} {local.Name}"};
            }
        }
    }
}