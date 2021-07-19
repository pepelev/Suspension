using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator.Generator
{
    internal sealed class Graph3 : IEnumerable<(string Suspension, Scope References)>
    {
        private readonly ControlFlowGraph graph;

        public Graph3(ControlFlowGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerator<(string Suspension, Scope References)> GetEnumerator()
        {
            var ways = new Graph(graph).ToLookup(pair => pair.From, pair => pair.To);
            var usages = new Graph2(graph, new ScopeUsage())
                .ToDictionary(pair => pair.Suspension, pair => pair.Scope);

            var declarations = new Graph4(graph).ToDictionary(pair => pair.Suspension, pair => pair.Declaration);
            var result = new Dictionary<string, Lazy<Scope>>();
            foreach (var pair in usages)
            {
                var reachable = ways[pair.Key].Except(new[] {pair.Key});
                result.Add(
                    pair.Key,
                    new Lazy<Scope>(
                        () => reachable.Aggregate(
                            pair.Value,
                            (scope, name) => scope.Union(result[name].Value)
                        ).Except(declarations[pair.Key])
                    )
                );
            }

            return result.Select(pair => (pair.Key, pair.Value.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}