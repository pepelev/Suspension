using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class Graph2 : IEnumerable<(string Suspension, Scope Scope)>
    {
        private readonly ControlFlowGraph graph;
        private readonly OperationVisitor<Scope, Scope> scopeVisitor;

        public Graph2(ControlFlowGraph graph, OperationVisitor<Scope, Scope> scopeVisitor)
        {
            this.graph = graph;
            this.scopeVisitor = scopeVisitor;
        }

        public IEnumerator<(string Suspension, Scope Scope)> GetEnumerator()
        {
            var names = new Graph(graph).SelectMany(pair => new[] { pair.From, pair.To }).Distinct().ToList();
            foreach (var name in names)
            {
                var startPoint = Find(name);
                var scope = ConstantScope.Empty;
                var visited = new HashSet<FlowPoint>();
                var queue = new Queue<FlowPoint>();
                queue.Enqueue(startPoint);

                m1: while (queue.Count > 0)
                {
                    var point = queue.Dequeue();
                    var block = point.Block;

                    for (var i = point.Index; i < block.Operations.Length; i++)
                    {
                        var currentPoint = new FlowPoint(block, i);
                        if (visited.Contains(currentPoint))
                            goto m1;

                        var operation = block.Operations[i];
                        if (operation.Accept(new SuspensionPoint.Is()))
                            goto m1;

                        scope = operation.Accept(scopeVisitor, scope);
                        visited.Add(currentPoint);
                    }

                    if (block.ConditionalSuccessor is { } conditional)
                    {
                        scope = block.BranchValue.Accept(scopeVisitor, scope);
                        queue.Enqueue(new FlowPoint(conditional.Destination));
                    }

                    if (block.FallThroughSuccessor is { } fallThrough)
                    {
                        queue.Enqueue(new FlowPoint(fallThrough.Destination));
                    }
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