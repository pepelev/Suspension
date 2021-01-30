using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Suspension.SourceGenerator.Generator;

namespace Suspension.SourceGenerator
{
    [Generator]
    public sealed class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var compilation = context.Compilation;
                var coroutines = compilation.SyntaxTrees.SelectMany(tree => new Coroutines(tree, compilation));
                foreach (var coroutine in coroutines)
                {
                    var tree = coroutine.Document;
                    context.AddSource(tree.FilePath.Replace(":", "."), tree.GetText());
                }
            }
            catch (Exception e)
            {
                throw;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "SSP0001",
                        "Title",
                        "Hello",
                        "Debg",
                        DiagnosticSeverity.Warning,
                        true
                    ),
                    null
                )
            );
        }
    }
}