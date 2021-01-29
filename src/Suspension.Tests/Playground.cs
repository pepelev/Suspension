﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Suspension.SourceGenerator;
using Suspension.Tests.Samples.Cycles;
using static Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines;

namespace Suspension.Tests
{
    public class Playground
    {
        [Test]
        public void Test()
        {
            var code = File.ReadAllText("Samples/Cycles/While.cs", Encoding.UTF8);
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
            var syntaxTrees = new Coroutines2(tree, compilation).ToList();

            Console.WriteLine(
                string.Join(
                    Environment.NewLine + Environment.NewLine,
                    syntaxTrees
                )
            );
        }

        [Test]
        public void TestCondition()
        {
            var code = File.ReadAllText("Samples/Conditions/Class.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\3.1.9\\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(typeof(Coroutine<>).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            foreach (var syntaxTree in new Coroutines(compilation))
            {
                var path = Path.Combine("Samples/Conditions", syntaxTree.FilePath);
                var expectedCode = File.ReadAllText(path, Encoding.UTF8);
                Assert.AreEqual(expectedCode, syntaxTree.ToString());
            }
        }

        [Test]
        public void TestExceptions()
        {
            var code = File.ReadAllText("Samples/Exceptions/TryFinally.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile("C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\3.1.9\\System.Runtime.dll"),
                    MetadataReference.CreateFromFile(typeof(Coroutine<>).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            foreach (var syntaxTree in new Coroutines(compilation))
            {
            }
        }

        [Test]
        public void Execution()
        {
            var random = new Random(142);
            Coroutine<None> execute = new Execute.Entry(Console.WriteLine, () => random.Next(10) > 3);
            while (!execute.Completed)
            {
                execute = execute.Run();
            }
        }
    }
}