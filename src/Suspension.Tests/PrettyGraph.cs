using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.Tests
{
    public sealed class PrettyGraph
    {
        private readonly ControlFlowGraph graph;

        public PrettyGraph(ControlFlowGraph graph)
        {
            this.graph = graph;
        }

        public override string ToString() => string.Join(
            Environment.NewLine + Environment.NewLine,
            graph.Blocks.Select(block => new Block(block))
        );

        private sealed class Block
        {
            private readonly BasicBlock block;

            public Block(BasicBlock block)
            {
                this.block = block;
            }

            public override string ToString()
            {
                if (block.Kind == BasicBlockKind.Entry)
                {
                    return $"{block.Ordinal}: Entry -> {block.FallThroughSuccessor.Destination.Ordinal}";
                }

                if (block.Kind == BasicBlockKind.Exit)
                {
                    return $"{block.Ordinal}: {(block.IsReachable ? "[reachable]" : "[unreachable]")} Exit";
                }

                var header = $"{block.Ordinal}: {(block.IsReachable ? "[reachable]" : "[unreachable]")}";
                var scope = $"scope: {string.Join(", ", Scope(block.EnclosingRegion))}";
                var region = $"region: {string.Join(", ", Region(block.EnclosingRegion))}";
                var syntaxNodes = block.Operations.Select(operation => operation.Syntax).ToList();
                var body = syntaxNodes.Count > 0
                    ? "body:" + Environment.NewLine + string.Join(Environment.NewLine, syntaxNodes)
                    : "body: []";

                return string.Join(
                    Environment.NewLine,
                    Successors().Prepend(body).Prepend(region).Prepend(scope).Prepend(header)
                );

                IEnumerable<string> Successors()
                {
                    if (block.ConditionalSuccessor is { } conditional)
                    {
                        yield return $"{conditional.Destination.Ordinal} <- {block.ConditionKind} {block.BranchValue.Syntax}";
                    }

                    if (block.FallThroughSuccessor is { } fallThrough)
                    {
                        if (fallThrough.Semantics == ControlFlowBranchSemantics.Return)
                        {
                            yield return $"<- return {block.BranchValue.Syntax}";
                        }
                        else if (fallThrough.Semantics == ControlFlowBranchSemantics.Throw)
                        {
                            yield return $"<- throw {block.BranchValue.Syntax}";
                        }
                        else if (fallThrough.Destination != null)
                        {
                            yield return $"{fallThrough.Destination.Ordinal} <- {fallThrough.Semantics}";
                        }
                        else if (block.BranchValue != null)
                        {
                            yield return $"{block.BranchValue.Syntax} <|> {fallThrough.Semantics}";
                        }
                        else
                        {
                            yield return $"> {fallThrough.Semantics}";
                        }
                    }
                }
            }

            private static IEnumerable<string> Region(ControlFlowRegion region)
            {
                for (var target = region; target != null; target = target.EnclosingRegion)
                {
                    var description = target.ExceptionType switch
                    {
                        { } type => $"{target.Kind} {type}",
                        null => target.Kind.ToString()
                    };
                    yield return description;
                }
            }

            private static IEnumerable<string> Scope(ControlFlowRegion region)
            {
                if (region.EnclosingRegion is { } parent)
                {
                    foreach (var item in Scope(parent))
                    {
                        yield return item;
                    }
                }

                foreach (var local in region.Locals)
                {
                    yield return $"{local.Type} {local.Name}";
                }
            }
        }
    }
}