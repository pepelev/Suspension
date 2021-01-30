using System;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    internal readonly struct FlowPoint : IEquatable<FlowPoint>
    {
        public FlowPoint(BasicBlock block, int index = 0)
        {
            Block = block;
            Index = index;
        }

        public BasicBlock Block { get; }
        public int Index { get; }
        public bool Equals(FlowPoint other) => Block.Equals(other.Block) && Index == other.Index;
        public override bool Equals(object obj) => obj is FlowPoint other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Block.GetHashCode() * 397) ^ Index;
            }
        }
    }
}