using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using NUnit.Framework;
using Suspension.SourceGenerator.Domain.Second;
using Suspension.SourceGenerator.Generator;
using Suspension.SourceGenerator.Predicates;

namespace Suspension.Tests
{
    public sealed class FlowReachabilityShould
    {
        [Test]
        [TestCase("Samples/While.cs", "action(\"Hello\");", "action(\"Hello\");", ExpectedResult = true)]
        [TestCase("Samples/While.cs", "action(\"Hello\");", "action(\"visited check\");", ExpectedResult = true)]
        [TestCase("Samples/While.cs", "action(\"visited check\");", "action(\"Hello\");", ExpectedResult = false)]
        [TestCase("Samples/While.cs", "action(\"Hello\");", "action(\"before outside\");", ExpectedResult = true)]
        [TestCase("Samples/While.cs", "action(\"before outside\");", "action(\"World\");", ExpectedResult = false)]
        [TestCase("Samples/SingleTryCatch.cs", "action(\"start\");", "action(\"Ok\");", ExpectedResult = true)]
        [TestCase("Samples/SingleTryCatch.cs", "action(\"start\");", "action(\"No ok\");", ExpectedResult = true)]
        [TestCase("Samples/SingleTryCatch.cs", "action(\"Ok\");", "action(\"No ok\");", ExpectedResult = false)]
        [TestCase("Samples/SingleTryCatch.cs", "action(\"Ok\");", "action($\"throw {e.Message}\");", ExpectedResult = true)]
        [TestCase("Samples/SingleTryCatch.cs", "action(\"No ok\");", "action($\"throw {e.Message}\");", ExpectedResult = true)]
        [TestCase("Samples/SingleTryCatch.cs", "action($\"throw {e.Message}\");", "action(\"No ok\");", ExpectedResult = false)]
        [TestCase("Samples/TryFinally.cs", "action(\"start\");", "action(\"argument false\");", ExpectedResult = true)]
        [TestCase("Samples/TryFinally.cs", "action(\"start\");", "action(\"finally\");", ExpectedResult = true)]
        [TestCase("Samples/TryFinally.cs", "action(\"argument false\");", "action(\"finally\");", ExpectedResult = true)]
        [TestCase("Samples/TryFinally.cs", "action(\"Ok\");", "action(\"finally\");", ExpectedResult = true)]
        [TestCase("Samples/TryFinally.cs", "action(\"finally\");", "action(\"Ok\");", ExpectedResult = false)]
        [TestCase("Samples/TryFinally.cs", "action(\"finally\");", "action(\"argument false\");", ExpectedResult = false)]
        [TestCase("Samples/NestedTryFinally.cs", "action(\"start\");", "action(\"inner try\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryFinally.cs", "action(\"inner try\");", "action(\"outer finally\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryFinally.cs", "action(\"inner finally\");", "action(\"outer finally\");", ExpectedResult = true)]
        [TestCase("Samples/EmptyTryFinally.cs", "action(\"start\");", "action(\"finally\");", ExpectedResult = true)]
        [TestCase("Samples/EmptyTryFinally.cs", "action(\"finally\");", "action(\"start\");", ExpectedResult = false)]
        [TestCase("Samples/NestedTryCatch.cs", "action(\"start\");", "action(\"inner try\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatch.cs", "action(\"start\");", "action(\"inner catch\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatch.cs", "action(\"start\");", "action(\"throw\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatch.cs", "action(\"inner try\");", "action(\"inner catch\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatch.cs", "action(\"inner try\");", "action(\"throw\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatch.cs", "action(\"inner catch\");", "action(\"throw\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatchWithEmptyCatch.cs", "action(\"start\");", "action(\"inner try\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatchWithEmptyCatch.cs", "action(\"start\");", "action(\"throw\");", ExpectedResult = true)]
        [TestCase("Samples/NestedTryCatchWithEmptyCatch.cs", "action(\"inner try\");", "action(\"throw\");", ExpectedResult = true)]
        public bool LookupReachablePoints(string path, string entryCode, string targetCode)
        {
            var graph = Graph(path);

            var entry = Locate(graph, entryCode);
            var target = Locate(graph, targetCode);
            var reachability = new FlowReachability(entry, graph, new SuspensionPoint.Is());

            return reachability.Reachable(target);
        }

        private static FlowPoint Locate(ControlFlowGraph graph, string code)
        {
            foreach (var block in graph.Blocks)
            {
                for (var i = 0; i < block.Operations.Length; i++)
                {
                    var operation = block.Operations[i];
                    if (operation.Syntax.ToString() == code)
                    {
                        return new FlowPoint(block, i);
                    }
                }
            }

            throw new ArgumentException("Could not find code");
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
                    MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\3.1.13\\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(typeof(Coroutine).Assembly.Location)
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