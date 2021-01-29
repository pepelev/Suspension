using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using NUnit.Framework;
using Suspension.SourceGenerator;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Generator;
using Suspension.SourceGenerator.Predicates;

namespace Suspension.Tests
{
    public class GraphsShould
    {
        [Test]
        public void Test()
        {
            var graph = Graph("Samples/While.cs");
            var valueTuples = new Graph(graph).ToList();
            valueTuples.Should().BeEquivalentTo(
                ("Entry", "InsideWhile"),
                ("Entry", "OutsideWhile"),
                ("InsideWhile", "InsideWhile"),
                ("InsideWhile", "OutsideWhile"),
                ("OutsideWhile", "Exit")
            );
        }

        [Test]
        [Explicit("Assert scope correct")]
        [TestCase("SimpleIf")]
        [TestCase("While")]
        public void Test2(string @class)
        {
            var graph = Graph($"Samples/{@class}.cs");
            var valueTuples = new Graph3(graph).ToList();
            valueTuples.Should().BeEquivalentTo(
                ("Entry", null as Scope),
                ("InsideIf", null as Scope),
                ("OutsideIf", null as Scope),
                ("Exit", null as Scope)
            );
        }

        private static ControlFlowGraph Graph(string path)
        {
            var code = File.ReadAllText(path, Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\3.1.11\\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(typeof(Coroutine<>).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var semantic = compilation.GetSemanticModel(tree);
            var method = tree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .That(
                    new HasAttribute(semantic, new FullName("global::Suspension.SuspendableAttribute"))
                )
                .Single();
            return ControlFlowGraph.Create(method, semantic);
        }
    }
}