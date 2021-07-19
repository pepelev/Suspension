using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using NUnit.Framework;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Domain.Values;
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
        [TestCase(nameof(Throw), nameof(Throw.SimpleThrow))]
        [TestCase(nameof(Throw), nameof(Throw.ThrowFromIf))]
        [TestCase(nameof(Throw), nameof(Throw.TryCatch))]
        [TestCase(nameof(Throw), nameof(Throw.TryCatchException))]
        [TestCase(nameof(Throw), nameof(Throw.TryCatchWhen))]
        [TestCase(nameof(Throw), nameof(Throw.TryTwoCatch))]
        [TestCase(nameof(Throw), nameof(Throw.TryTwoCatchWhen))]
        [TestCase(nameof(Throw), nameof(Throw.Using))]
        [TestCase(nameof(Throw), nameof(Throw.UsingNull))]
        [TestCase(nameof(Throw), nameof(Throw.TryCatchFinally))]
        [TestCase(nameof(Countdown2), nameof(Countdown2.WriteToConsole))]
        [TestCase(nameof(Countdown2), nameof(Countdown2.Out))]
        [TestCase(nameof(Countdown2), nameof(Countdown2.Declare))]
        [TestCase(nameof(Countdown2), nameof(Countdown2.Declare2))]
        public void Pretty(string className, string methodName)
        {
            var graph = Graph(className, methodName);
            var pretty = new PrettyGraph(graph);
            Console.WriteLine(pretty);
        }

        public static TestCaseData[] Cases =
        {
            new TestCaseData(
                nameof(Countdown2),
                nameof(Countdown2.WriteToConsole),
                new[]
                {
                    ("Entry", new string[0]),
                    ("Cycle", new string[0]),
                    ("Exit", new string[0])
                }
            ),
            new TestCaseData(
                nameof(Countdown2),
                nameof(Countdown2.Out),
                new[]
                {
                    ("Entry", new[] {"d"}),
                    ("Middle", new string[0]),
                    ("Exit", new string[0])
                }
            ),
        };

        [Test]
        [TestCaseSource(nameof(Cases))]
        public void Test3(string className, string methodName, (string, string[])[] expected)
        {
            var graph = Graph(className, methodName);
            var graph4 = new Graph4(graph).ToList();

            graph4.Select(x => (x.Suspension, x.Declaration.Select(y => y.OriginalName).ToArray())).Should().BeEquivalentTo(
                expected
            );
        }

        private static ControlFlowGraph Graph(string className, string methodName)
        {
            var code = File.ReadAllText($"Samples/{className}.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\mscorlib.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Core.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Console.dll"),
                    MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Xml.dll"),
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
            return graph;
        }
    }
}