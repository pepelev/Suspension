using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using NUnit.Framework;
using Suspension.SourceGenerator.Generator;
using Suspension.Tests.Samples;
using static Suspension.Tests.Samples.While.Coroutines;

namespace Suspension.Tests
{
    [Explicit("For manual running")]
    public class Playground
    {
        [Test]
        public void Test()
        {
            var code = File.ReadAllText("Samples/While.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\3.1.11\\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(typeof(Coroutine).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            var syntaxTrees = new Coroutines(tree, compilation).ToList();

            Console.WriteLine(
                string.Join(
                    Environment.NewLine + Environment.NewLine,
                    syntaxTrees
                )
            );
        }

        [Test]
        public void Execution()
        {
            var random = new Random(142);
            Coroutine execute = new Execute.Entry(() => random.Next(10) > 3, Console.WriteLine);
            while (!execute.Completed)
            {
                execute = execute.Run();
            }
        }

        [Test]
        [TestCase(nameof(Throw.SimpleThrow))]
        [TestCase(nameof(Throw.ThrowFromIf))]
        [TestCase(nameof(Throw.TryCatch))]
        [TestCase(nameof(Throw.TryCatchException))]
        [TestCase(nameof(Throw.TryCatchWhen))]
        [TestCase(nameof(Throw.TryTwoCatch))]
        [TestCase(nameof(Throw.TryTwoCatchWhen))]
        [TestCase(nameof(Throw.Using))]
        [TestCase(nameof(Throw.UsingNull))]
        [TestCase(nameof(Throw.TryCatchFinally))]
        public void Pretty(string methodName)
        {
            var code = File.ReadAllText("Samples/Throw.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] { tree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.11\mscorlib.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.11\System.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.11\System.Core.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.11\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.11\System.Console.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.11\System.Xml.dll"),
                    MetadataReference.CreateFromFile(typeof(Coroutine).Assembly.Location),
                    MetadataReference.CreateFromFile("FluentAssertions.dll"),
                    MetadataReference.CreateFromFile("nunit.framework.dll")
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var result = compilation.Emit(Stream.Null);
            if (!result.Success)
                throw new Exception("Compilation failed");

            var semantic = compilation.GetSemanticModel(tree);
            var syntax = tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single(method => semantic.GetDeclaredSymbol(method)?.Name == methodName);

            var graph = ControlFlowGraph.Create(syntax, semantic);
            var pretty = new PrettyGraph(graph);
            Console.WriteLine(pretty);
        }
    }
}