using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class Graph4 : IEnumerable<(string Suspension, Scope Declaration)>
    {
        private readonly ControlFlowGraph graph;

        public Graph4(ControlFlowGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerator<(string Suspension, Scope Declaration)> GetEnumerator()
        {
            var visited = new HashSet<FlowPoint>();
            var visitedBlocks = new HashSet<BasicBlock>();
            var used = new HashSet<ILocalSymbol>();
            var dct = new Dictionary<string, Scope>();

            var stack = new Stack<(FlowPoint, string)>(
                new[] {(Entry, "Entry")}
            );

            while (stack.Count > 0)
            {
                var (point, name) = stack.Pop();
                if (visited.Contains(point))
                {
                    continue;
                }

                var block = point.Block;
                if (!visitedBlocks.Contains(block))
                {
                    var locals = block.EnclosingRegion.Hierarchy().SelectMany(region => region.Locals).ToList();
                    used.UnionWith(locals);
                    if (!dct.ContainsKey(name))
                    {
                        dct[name] = ConstantScope.Empty;
                    }

                    dct[name] = dct[name].Union(locals.Select(local => new LocalValue(local)));

                    visitedBlocks.Add(block);
                }

                if (point.AtOperation)
                {
                    if (point.Operation.Accept(new SuspensionPoint.Is()))
                    {
                        var nextName = point.Operation.Accept(new SuspensionPoint.Name());
                        if (!dct.ContainsKey(nextName))
                        {
                            dct[nextName] = ConstantScope.Empty;
                        }

                        stack.Push((point.Next, nextName));
                    }
                    else
                    {
                        stack.Push((point.Next, name));
                    }
                }
                else
                {
                    foreach (var successor in point.Block.Successors())
                    {
                        stack.Push((new FlowPoint(successor), name));
                    }
                }

                visited.Add(point);
            }

            return dct
                .Select(pair => (pair.Key, pair.Value))
                .Append(Exit)
                .GetEnumerator();
        }

        private static (string Suspension, Scope Declaration) Exit => ("Exit", ConstantScope.Empty);
        private FlowPoint Entry => new(graph.Entry());
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}