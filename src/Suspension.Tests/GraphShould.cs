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
using Suspension.SourceGenerator.Predicates;

namespace Suspension.Tests
{
    public class GraphShould
    {
        [Test]
        public void Test()
        {
            var code = File.ReadAllText("Samples/Cycles/While.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] { tree },
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
                    new HasAttribute(semantic, new FullName("Suspension.SuspendableAttribute"))
                )
                .Single();
            var graph = ControlFlowGraph.Create(method, semantic);
            new Graph(graph).Should().BeEquivalentTo(
                ("Entry", "InsideWhile"),
                ("Entry", "OutsideWhile"),
                ("InsideWhile", "InsideWhile"),
                ("InsideWhile", "OutsideWhile"),
                ("OutsideWhile", "Exit")
            );
        }
    }
}