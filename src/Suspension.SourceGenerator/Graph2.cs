using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator
{
    internal sealed class Graph2 : IEnumerable<(string Suspension, Scope ShallowReferences)>
    {
        private readonly ControlFlowGraph graph;

        public Graph2(ControlFlowGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerator<(string Suspension, Scope ShallowReferences)> GetEnumerator()
        {
            var names = new Graph(graph).SelectMany(pair => new[] { pair.From, pair.To }).Distinct().ToList();
            foreach (var name in names)
            {
                var startPoint = Find(name);
                var scope = ConstantScope.Empty;
                var visitor = new ScopeUsage();
                var visited = new HashSet<BasicBlock>();
                var queue = new Queue<FlowPoint>();
                queue.Enqueue(startPoint);

                while (queue.Count > 0)
                {
                    var point = queue.Dequeue();
                    var block = point.Block;
                    if (visited.Contains(block))
                        continue;

                    for (var i = point.Index; i < block.Operations.Length; i++)
                    {
                        var operation = block.Operations[i];
                        if (operation.Accept(new SuspensionPoint.Is()))
                            goto m1;

                        scope = operation.Accept(visitor, scope);
                    }

                    if (block.ConditionalSuccessor is { } conditional)
                    {
                        scope = block.BranchValue.Accept(visitor, scope);
                        queue.Enqueue(new FlowPoint(conditional.Destination, 0));
                    }

                    if (block.FallThroughSuccessor is { } fallThrough)
                    {
                        queue.Enqueue(new FlowPoint(fallThrough.Destination, 0));
                    }

                    m1: visited.Add(block);
                }

                yield return (name, scope);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private FlowPoint Find(string suspensionPoint)
        {
            if (suspensionPoint == "Entry")
                return new FlowPoint(graph.Entry(), 0);

            if (suspensionPoint == "Exit")
                return new FlowPoint(graph.Exit(), 0);

            foreach (var block in graph.Blocks)
            {
                for (var i = 0; i < block.Operations.Length; i++)
                {
                    var operation = block.Operations[i];
                    if (operation.Accept(new SuspensionPoint.Is()))
                    {
                        var name = operation.Accept(new SuspensionPoint.Name());
                        if (name == suspensionPoint)
                            return new FlowPoint(block, i + 1);
                    }
                }
            }

            throw new InvalidOperationException();
        }
    }
}