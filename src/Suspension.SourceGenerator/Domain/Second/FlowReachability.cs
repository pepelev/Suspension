using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Suspension.SourceGenerator.Generator;

namespace Suspension.SourceGenerator.Domain.Second
{
    internal sealed class FlowReachability
    {
        private readonly ControlFlowGraph graph;
        private readonly Lazy<HashSet<FlowPoint>> reachable;

        public FlowReachability(FlowPoint entry, ControlFlowGraph graph, OperationVisitor<None, bool> suspensionPoint)
        {
            this.graph = graph;
            if (!Belongs(entry))
            {
                throw new ArgumentException("Entry not belongs to graph");
            }

            reachable = new Lazy<HashSet<FlowPoint>>(
                () =>
                {
                    var visited = new HashSet<FlowPoint>();
                    var visitedRegions = new HashSet<ControlFlowRegion>();
                    var stack = new Stack<FlowPoint>();
                    stack.Push(entry);
                    while (stack.Count > 0)
                    {
                        var point = stack.Pop();
                        if (!visited.Add(point))
                        {
                            continue;
                        }

                        var region = point.Block.EnclosingRegion;
                        if (visitedRegions.Add(region))
                        {
                            var tries = region.Containing(ControlFlowRegionKind.Try).Take(1);
                            foreach (var @try in tries)
                            {
                                var parent = @try.EnclosingRegion;
                                if (parent.Kind == ControlFlowRegionKind.TryAndCatch)
                                {
                                    if (visitedRegions.Add(parent))
                                    {
                                        var catchRegion = parent.NestedRegions[1];
                                        var @catch = new FlowPoint(graph.Blocks[catchRegion.FirstBlockOrdinal]);
                                        stack.Push(@catch);
                                    }
                                }
                                else if (parent.Kind == ControlFlowRegionKind.TryAndFinally)
                                {
                                    if (visitedRegions.Add(parent))
                                    {
                                        var finallyRegion = parent.NestedRegions[1];
                                        var @finally = new FlowPoint(graph.Blocks[finallyRegion.FirstBlockOrdinal]);
                                        stack.Push(@finally);
                                    }
                                }
                            }
                        }

                        if (point.AtOperation)
                        {
                            var operation = point.Operation;
                            if (!suspensionPoint.Visit(operation))
                            {
                                stack.Push(point.Next);
                            }

                            continue;
                        }

                        if (point.Block.FallThroughSuccessor is { Destination: { } fallThrough })
                        {
                            stack.Push(new FlowPoint(fallThrough));
                        }

                        if (point.Block.ConditionalSuccessor is {Destination: { } conditional})
                        {
                            stack.Push(new FlowPoint(conditional));
                        }

                    }

                    return visited;
                },
                LazyThreadSafetyMode.None
            );
        }

        public bool Reachable(FlowPoint target)
        {
            if (!Belongs(target))
            {
                throw new ArgumentException("Point not belongs to graph");
            }

            return reachable.Value.Contains(target);
        }

        private bool Belongs(FlowPoint target)
        {
            var ordinal = target.Block.Ordinal;
            if (ordinal >= graph.Blocks.Length)
            {
                return false;
            }

            var graphBlock = graph.Blocks[ordinal];
            return graphBlock.Equals(target.Block);
        }
    }
}