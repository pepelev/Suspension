using System;
using System.IO;
using System.Text;
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
        public void Literal(string preprocessingDirective)
        {
            AssertNoDiagnostics("Syntax.cs", "Literal", preprocessingDirective);
        }

        [Test]
        [TestCase("ObjectCreation_Object")]
        [TestCase("ObjectCreation_String")]
        [TestCase("ObjectCreation_ArrayOfStrings")]
        [TestCase("ObjectCreation_ArrayOfStringsWithInitializer")]
        [TestCase("ObjectCreation_MultidimensionalArrayOfStrings")]
        [TestCase("ObjectCreation_ListOfStrings")]
        public void ObjectCreation(string preprocessingDirective)
        {
            AssertNoDiagnostics("Syntax.cs", "ObjectCreation", preprocessingDirective);
        }

        [Test]
        [TestCase("ObjectCreation_Object")]
        [TestCase("ObjectCreation_String")]
        [TestCase("ObjectCreation_ListOfStrings")]
        public void MethodCall(string preprocessingDirective)
        {
            AssertNoDiagnostics("Syntax.cs", "MethodCall", preprocessingDirective);
        }

        [Test]
        public void KeywordVariableName()
        {
            AssertNoDiagnostics("Syntax.cs", "KeywordVariableName");
        }

        private static void AssertNoDiagnostics(string fileName, params string[] preprocessingDirectives)
        {
            var code = File.ReadAllText(fileName, Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(
                code,
                CSharpParseOptions.Default
                    .WithPreprocessorSymbols(preprocessingDirectives)
            );
            var compilation = CSharpCompilation.Create(
                "Suspension.Tests.Samples",
                new[] {tree},
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Lazy<,>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Coroutine).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var driver = CSharpGeneratorDriver.Create(new SourceGenerator.SourceGenerator());
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var cmp, out var diagnostics);
            var emitResult = cmp.Emit(Stream.Null);
            emitResult.Diagnostics.Should().BeEmpty();
            diagnostics.Should().BeEmpty();
        }
    }
}