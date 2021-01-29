using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    internal readonly struct FlowPoint
    {
        public FlowPoint(BasicBlock block, int index = 0)
        {
            Block = block;
            Index = index;
        }

        public BasicBlock Block { get; }
        public int Index { get; }
    }
}