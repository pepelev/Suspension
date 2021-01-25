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
                    new HasAttribute(semantic, new FullName("Suspension.SuspendableAttribute"))
                )
                .Single();
            var graph = ControlFlowGraph.Create(method, semantic);
            var valueTuples = new Graph(graph).ToList();

            return syntaxNodes
                .OfType<MethodDeclarationSyntax>()
                .That(
                    new HasAttribute(semantic, new FullName("Suspension.SuspendableAttribute"))
                )
                .SelectMany(method => MakeSuspendable(method, semantic))
                .GetEnumerator();
        }

        private IEnumerable<(string From, string To)> Graph(MethodDeclarationSyntax method, SemanticModel semantic)
        {
            var graph = ControlFlowGraph.Create(method, semantic);
            var entry = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Entry);
            var exit = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Exit);

            var suspensionPoints = graph.Blocks.Select(
                    block => new
                    {
                        Block = block,
                        SuspensionPoints = block.Operations
                            .That(new SuspensionPoint.Is())
                            .Select(new SuspensionPoint.Name())
                            .ToList()
                    }
                )
                .Prepend(new {Block = entry, SuspensionPoints = new List<string> {"Entry"}})
                .Append(new {Block = exit, SuspensionPoints = new List<string> {"Exit"}})
                .Where(pair => pair.SuspensionPoints.Any())
                .ToDictionary(pair => pair.Block, pair => pair.SuspensionPoints);

            foreach (var names in suspensionPoints.Values)
            {
                foreach (var pair in names.Pairwise())
                {
                    yield return pair;
                }
            }

            foreach (var pair in suspensionPoints)
            {
                var startBlock = pair.Key;
                var points = pair.Value;

                var visited = new HashSet<BasicBlock> {startBlock};
                var queue = new Queue<BasicBlock>();
                {
                    if (startBlock.ConditionalSuccessor is { } conditional)
                    {
                        queue.Enqueue(conditional.Destination);
                    }

                    if (startBlock.FallThroughSuccessor is { } fallThrough)
                    {
                        queue.Enqueue(fallThrough.Destination);
                    }
                }

                while (queue.Count > 0)
                {
                    var block = queue.Dequeue();

                    if (suspensionPoints.TryGetValue(block, out var list))
                    {
                        yield return (points.Last(), list[0]);
                    }
                    else
                    {
                        if (visited.Contains(block))
                            continue;

                        if (block.ConditionalSuccessor is { } conditional)
                        {
                            queue.Enqueue(conditional.Destination);
                        }

                        if (block.FallThroughSuccessor is { } fallThrough)
                        {
                            queue.Enqueue(fallThrough.Destination);
                        }
                    }

                    visited.Add(block);
                }
            }
        }

        private IEnumerable<SyntaxTree> MakeSuspendable(MethodDeclarationSyntax method, SemanticModel semantic)
        {
            var symbol = semantic.GetDeclaredSymbol(method) ?? throw new Exception("GetDeclaredSymbol failed");
            var graph = ControlFlowGraph.Create(method, semantic);
            var entry = graph.Blocks.Single(block => block.Kind == BasicBlockKind.Entry);

            var references = new Graph3(graph).ToDictionary(pair => pair.Suspension, pair => pair.References);

            // todo put this code into bfs class
            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<BasicBlock>();
            queue.Enqueue(entry);

            var result = new List<Dumb>();
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
                        result.Add(new Dumb(name, symbol, new FlowPoint(block, i + 1), references[name]));
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

            foreach (var dumb in result)
            {
                yield return dumb.Document;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}