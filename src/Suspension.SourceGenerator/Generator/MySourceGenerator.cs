using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    [Generator]
    public class MySourceGenerator : ISourceGenerator
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
            var compilation = context.Compilation;
            var trees = compilation.SyntaxTrees.SelectMany(tree => new Coroutines2(tree, compilation)).ToList();
            for (var i = 0; i < trees.Count; i++)
            {
                var tree = trees[i];
                context.AddSource(tree.FilePath.Replace(":", "."), tree.GetText());
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