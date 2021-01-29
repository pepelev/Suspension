using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Predicates;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class Coroutines : IEnumerable<Coroutine>
    {
        private readonly SyntaxTree document;
        private readonly Compilation compilation;

        public Coroutines(SyntaxTree document, Compilation compilation)
        {
            this.document = document;
            this.compilation = compilation;
        }

        public IEnumerator<Coroutine> GetEnumerator()
        {
            var semantic = compilation.GetSemanticModel(document);
            var syntaxNodes = document.GetRoot().DescendantNodes().ToList();
            return syntaxNodes
                .OfType<MethodDeclarationSyntax>()
                .That(
                    new HasAttribute(semantic, new FullName("global::Suspension.SuspendableAttribute"))
                )
                .SelectMany(method => MakeSuspendable(method, semantic))
                .GetEnumerator();
        }

        private IEnumerable<Coroutine> MakeSuspendable(MethodDeclarationSyntax method, SemanticModel semantic)
        {
            var symbol = semantic.GetDeclaredSymbol(method) ?? throw new Exception("GetDeclaredSymbol failed");
            var graph = ControlFlowGraph.Create(method, semantic);
            var entry = graph.Entry();

            var graph3 = new Graph3(graph);
            var references = graph3.ToDictionary(pair => pair.Suspension, pair => pair.References);

            Coroutine Dumb(string name, FlowPoint point)
            {
                return new Dumb(name, symbol, point, references[name], graph3);
            }

            yield return Dumb("Entry", new FlowPoint(entry));

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
                        yield return Dumb(name, new FlowPoint(block, i + 1));
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

            yield return new Exit(symbol);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}