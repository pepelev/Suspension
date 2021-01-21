using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator
{
    internal static class ControlFlowGraphExtensions
    {
        public static BasicBlock Entry(this ControlFlowGraph graph) => graph.Find(BasicBlockKind.Entry);
        public static BasicBlock Exit(this ControlFlowGraph graph) => graph.Find(BasicBlockKind.Exit);

        private static BasicBlock Find(this ControlFlowGraph graph, BasicBlockKind kind) =>
            graph.Blocks.Single(block => block.Kind == kind);
    }
}