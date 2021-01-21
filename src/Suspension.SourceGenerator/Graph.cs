using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Predicates;

namespace Suspension.SourceGenerator
{
    public sealed class Graph : IEnumerable<(string From, string To)>
    {
        private Lazy<Dictionary<BasicBlock, List<string>>> suspensionPoints;

        public Graph(ControlFlowGraph graph)
        {
            suspensionPoints = new Lazy<Dictionary<BasicBlock, List<string>>>(
                () => graph.Blocks.Select(
                        block => new
                        {
                            Block = block,
                            SuspensionPoints = block.Operations
                                .That(new SuspensionPoint.Is())
                                .Select(new SuspensionPoint.Name())
                                .ToList()
                        }
                    )
                    .Prepend(new { Block = graph.Entry(), SuspensionPoints = new List<string> { "Entry" } })
                    .Append(new { Block = graph.Exit(), SuspensionPoints = new List<string> { "Exit" } })
                    .Where(pair => pair.SuspensionPoints.Any())
                    .ToDictionary(pair => pair.Block, pair => pair.SuspensionPoints)
            );
        }

        public IEnumerator<(string From, string To)> GetEnumerator()
        {
            var inner = suspensionPoints.Value.Values.SelectMany(names => names.Pairwise());
            var outer = suspensionPoints.Value.SelectMany(Outer);
            return inner.Concat(outer).GetEnumerator();
        }

        private IEnumerable<(string From, string To)> Outer(KeyValuePair<BasicBlock, List<string>> pair)
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

                if (suspensionPoints.Value.TryGetValue(block, out var list))
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}