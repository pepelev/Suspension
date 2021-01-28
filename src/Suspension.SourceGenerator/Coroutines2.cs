using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Predicates;

namespace Suspension.SourceGenerator
{
    public sealed class Coroutines2 : IEnumerable<SyntaxTree>
    {
        private readonly SyntaxTree document;
        private readonly CSharpCompilation compilation;

        public Coroutines2(SyntaxTree document, CSharpCompilation compilation)
        {
            this.document = document;
            this.compilation = compilation;
        }

        public IEnumerator<SyntaxTree> GetEnumerator()
        {
            var emitResult = compilation.Emit(Stream.Null);
            if (!emitResult.Success)
            {
                throw new Exception($"Compilation failed with {string.Join("; ", emitResult.Diagnostics)}");
            }

            var semantic = compilation.GetSemanticModel(document);
            var syntaxNodes = document.GetRoot().DescendantNodes().ToList();

            var method = syntaxNodes
                .OfType<MethodDeclarationSyntax>()
                .That(
                    new HasAttribute(semantic, new FullName("global::Suspension.SuspendableAttribute"))
                )
                .Single();
            var graph = ControlFlowGraph.Create(method, semantic);
            var valueTuples = new Graph(graph).ToList();

            return syntaxNodes
                .OfType<MethodDeclarationSyntax>()
                .That(
                    new HasAttribute(semantic, new FullName("global::Suspension.SuspendableAttribute"))
                )
                .SelectMany(method => MakeSuspendable(method, semantic))
                .GetEnumerator();
        }

        private IEnumerable<SyntaxTree> MakeSuspendable(MethodDeclarationSyntax method, SemanticModel semantic)
        {
            var symbol = semantic.GetDeclaredSymbol(method) ?? throw new Exception("GetDeclaredSymbol failed");
            var graph = ControlFlowGraph.Create(method, semantic);
            var entry = graph.Entry();

            var graph3 = new Graph3(graph);
            var references = graph3.ToDictionary(pair => pair.Suspension, pair => pair.References);

            SyntaxTree Tree(string name, FlowPoint point)
            {
                var coroutine = new Dumb(name, symbol, point, references[name], graph3);
                return coroutine.Document;
            }

            yield return Tree("Entry", new FlowPoint(entry, 0));

            // todo put this code into bfs class
            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<BasicBlock>();
            queue.Enqueue(entry);

            while (queue.Count > 0)
            {
                var block = queue.Dequeue();
                if (visited.Contains(block))
                    continue;

                for (var i = 0; i < block.Operations.Length; i++)
                {
                    var operation = block.Operations[i];
                    if (operation.Accept(new SuspensionPoint.Is()))
                    {
                        var name = operation.Accept(new SuspensionPoint.Name());
                        yield return Tree(name, new FlowPoint(block, i + 1));
                    }
                }

                visited.Add(block);
                if (block.ConditionalSuccessor is { } conditional)
                {
                    queue.Enqueue(conditional.Destination);
                }

                if (block.FallThroughSuccessor is { } fallThrough)
                {
                    queue.Enqueue(fallThrough.Destination);
                }
            }

            yield return new Exit(symbol).Document;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}