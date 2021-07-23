using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class GraphAllSuspensionPoints : IEnumerable<(string Name, FlowPoint Point)>
    {
        private readonly ControlFlowGraph graph;

        public GraphAllSuspensionPoints(ControlFlowGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerator<(string Name, FlowPoint Point)> GetEnumerator()
        {
            var entry = graph.Entry();
            yield return ("Entry", new FlowPoint(entry));

            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<BasicBlock>(new[] {entry});

            while (queue.Count > 0)
            {
                var block = queue.Dequeue();
                for (var i = 0; i < block.Operations.Length; i++)
                {
                    var operation = block.Operations[i];
                    if (operation.Accept(new SuspensionPoint.Is()))
                    {
                        var name = operation.Accept(new SuspensionPoint.Name());
                        yield return (name, new FlowPoint(block, i + 1));
                    }
                }

                visited.Add(block);

                foreach (var successor in block.Successors().Without(visited))
                {
                    queue.Enqueue(successor);
                }
            }

            yield return ("Exit", new FlowPoint(graph.Exit()));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}