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
            // todo remove to list
            var points = new GraphAllSuspensionPoints(graph).ToList();
            foreach (var (name, startPoint) in points)
            {
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

                    if (block.FallThroughSuccessor?.Destination is { } destination)
                    {
                        queue.Enqueue(new FlowPoint(destination));
                    }
                }

                yield return (name, scope);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}