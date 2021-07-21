using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Suspension.Tests.Compilation
{
    public sealed class Tests
    {
        [Test]
        [TestCase("Literal_Int")]
        [TestCase("Literal_UInt")]
        [TestCase("Literal_Long")]
        [TestCase("Literal_ULong")]
        [TestCase("Literal_String")]
        [TestCase("Literal_Char")]
        [TestCase("Literal_Float")]
        [TestCase("Literal_Double")]
        [TestCase("Literal_Decimal")]
        public void Test(string preprocessingDirective)
        {
            var code = File.ReadAllText("Syntax.cs", Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(
                code,
                CSharpParseOptions.Default
                    .WithPreprocessorSymbols("Literal", preprocessingDirective)
            );
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] { tree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Lazy<,>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Coroutine).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var driver = CSharpGeneratorDriver.Create(new SourceGenerator.SourceGenerator());
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);
            diagnostics.Should().BeEmpty();
        }
    }
}