using System;
using System.IO;
using System.Linq;
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
        [TestCase("Literal", "Literal_Int")]
        [TestCase("Literal", "Literal_UInt")]
        [TestCase("Literal", "Literal_Long")]
        [TestCase("Literal", "Literal_ULong")]
        [TestCase("Literal", "Literal_String")]
        [TestCase("Literal", "Literal_Char")]
        [TestCase("Literal", "Literal_Float")]
        [TestCase("Literal", "Literal_Double")]
        [TestCase("Literal", "Literal_Decimal")]
        [TestCase("ParameterReference")]
        [TestCase("ObjectCreation", "ObjectCreation_Object")]
        [TestCase("ObjectCreation", "ObjectCreation_String")]
        [TestCase("ObjectCreation", "ObjectCreation_ArrayOfStrings")]
        [TestCase("ObjectCreation", "ObjectCreation_ArrayOfStringsWithInitializer")]
        [TestCase("ObjectCreation", "ObjectCreation_MultidimensionalArrayOfStrings")]
        [TestCase("ObjectCreation", "ObjectCreation_ListOfStrings")]
        [TestCase("KeywordVariableName")]
        [TestCase("ArrayElementReference", "ArrayElementReference_Regular")]
        [TestCase("ArrayElementReference", "ArrayElementReference_ArrayOfArrays")]
        [TestCase("ArrayElementReference", "ArrayElementReference_TwoDimensionalArray")]
        [TestCase("Assignment", "Assignment_Regular")]
        [TestCase("Assignment", "Assignment_Compound")]
        [TestCase("Assignment", "Assignment_Discard")]
        [TestCase("Assignment", "Assignment_Ref")]
        [TestCase("Assignment", "Assignment_Deconstruction")]
        [TestCase("Assignment", "Assignment_DeconstructionDeclaration")]
        [TestCase("MethodCall", "MethodCall_StaticMethod")]
        [TestCase("MethodCall", "MethodCall_InstanceMethod")]
        [TestCase("MethodCall", "MethodCall_RefParameter")]
        [TestCase("MethodCall", "MethodCall_OutParameter")]
        [TestCase("MethodCall", "MethodCall_OutVarParameter")]
        [TestCase("MethodCall", "MethodCall_InParameter")]
        [TestCase("MethodCall", "MethodCall_NamedParameters")]
        [TestCase("Declaration", "Declaration_Regular")]
        [TestCase("Declaration", "Declaration_Initialization")]
        [TestCase("Declaration", "Declaration_Var")]
        [TestCase("Declaration", "Declaration_Multiple")]
        [TestCase("Interpolation", "Interpolation_Regular")]
        [TestCase("Interpolation", "Interpolation_Format")]
        [TestCase("Interpolation", "Interpolation_Alignment")]
        [TestCase("Interpolation", "Interpolation_Format_Alignment")]
        public void Be_Compiled_Without_Diagnostics(string name, params string[] otherDirectives)
        {
            AssertNoDiagnostics("Syntax.cs", name, otherDirectives);
        }

        private static void AssertNoDiagnostics(string fileName, string methodName, params string[] preprocessingDirectives)
        {
            var code = File.ReadAllText(fileName, Encoding.UTF8);
            var tree = CSharpSyntaxTree.ParseText(
                code,
                CSharpParseOptions.Default
                    .WithPreprocessorSymbols(preprocessingDirectives.Prepend(methodName))
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
            cmp.ContainsSymbolsWithName(methodName, SymbolFilter.Member).Should().BeTrue();
        }
    }
}