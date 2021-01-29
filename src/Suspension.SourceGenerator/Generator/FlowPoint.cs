using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    internal readonly struct FlowPoint
    {
        public FlowPoint(BasicBlock block, int index)
        {
            Block = block;
            Index = index;
        }

        public BasicBlock Block { get; }
        public int Index { get; }
    }
}