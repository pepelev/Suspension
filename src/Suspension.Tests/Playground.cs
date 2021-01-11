using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Framework;

namespace Suspension.Tests
{
    public class Playground
    {
        [Test]
        public void Test()
        {
            var code = File.ReadAllText("Samples/JustSingleLineOfCode/Class.cs", Encoding.UTF8);
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
                var path = Path.Combine("Samples/JustSingleLineOfCode", syntaxTree.FilePath);
                var expectedCode = File.ReadAllText(path, Encoding.UTF8);
                Assert.AreEqual(expectedCode, syntaxTree.ToString());
            }
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

        public sealed class MethodParameters : OperationVisitor<None, IEnumerable<(string Type, string Name)>>
        {
            public override IEnumerable<(string Type, string Name)> DefaultVisit(IOperation operation, None argument)
            {
                throw new Exception($"V Visit failed {operation}");
            }

            public override IEnumerable<(string Type, string Name)> VisitInvocation(IInvocationOperation operation, None argument) =>
                from child in operation.Arguments.Append(operation.Instance)
                from result in child.Accept(this, argument)
                select result;

            public override IEnumerable<(string Type, string Name)> VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
            {
                return operation.Operation.Accept(this, argument);
            }

            public override IEnumerable<(string Type, string Name)> VisitArgument(IArgumentOperation operation, None argument)
            {
                return operation.Value.Accept(this, argument);
            }

            public override IEnumerable<(string Type, string Name)> VisitLiteral(ILiteralOperation operation, None argument)
            {
                return Array.Empty<(string Type, string Name)>();
            }

            public override IEnumerable<(string Type, string Name)> VisitParameterReference(IParameterReferenceOperation operation, None argument)
            {
                var parameter = operation.Parameter;
                return new[]
                {
                    (parameter.Type.ToString(), parameter.Name)
                };
            }

            public override IEnumerable<(string Type, string Name)> VisitSimpleAssignment(ISimpleAssignmentOperation operation, None argument)
            {
                return Array.Empty<(string Type, string Name)>();
            }

            public override IEnumerable<(string Type, string Name)> VisitLocalReference(ILocalReferenceOperation operation, None argument)
            {
                return Array.Empty<(string Type, string Name)>();
            }
        }

        public sealed class V : OperationVisitor<None, IEnumerable<string>>
        {
            public override IEnumerable<string> DefaultVisit(IOperation operation, None argument)
            {
                throw new Exception($"V Visit failed {operation}");
            }

            public override IEnumerable<string> VisitInvocation(IInvocationOperation operation, None argument) =>
                from child in operation.Arguments.Append(operation.Instance)
                from result in child.Accept(this, argument)
                select result;

            public override IEnumerable<string> VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
            {
                return operation.Operation.Accept(this, argument);
            }

            public override IEnumerable<string> VisitArgument(IArgumentOperation operation, None argument)
            {
                return operation.Value.Accept(this, argument);
            }

            public override IEnumerable<string> VisitLiteral(ILiteralOperation operation, None argument)
            {
                return Array.Empty<string>();
            }

            public override IEnumerable<string> VisitParameterReference(IParameterReferenceOperation operation, None argument)
            {
                var parameter = operation.Parameter;
                return new[] {$"parameter: {parameter.Type} {parameter.Name}"};
            }

            public override IEnumerable<string> VisitSimpleAssignment(ISimpleAssignmentOperation operation, None argument)
            {
                return Array.Empty<string>();
            }

            public override IEnumerable<string> VisitLocalReference(ILocalReferenceOperation operation, None argument)
            {
                var local = operation.Local;
                return new[] {$"local: {local.Type} {local.Name}"};
            }
        }
    }
}