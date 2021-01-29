﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Suspension.SourceGenerator.Generator;
using static Suspension.Tests.Samples.While.Coroutines;

namespace Suspension.Tests
{
    [Explicit("For manual running")]
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
            Coroutine<None> execute = new Execute.Entry(Console.WriteLine, () => random.Next(10) > 3);
            while (!execute.Completed)
            {
                execute = execute.Run();
            }
        }
    }
}