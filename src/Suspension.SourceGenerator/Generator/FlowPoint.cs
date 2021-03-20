using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    internal readonly struct FlowPoint : IEquatable<FlowPoint>
    {
        public FlowPoint(BasicBlock block, int index = 0)
        {
            if (index < 0 || block.Operations.Length < index)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "Index must point to operation or right after last operation"
                );
            }

            Block = block;
            Index = index;
        }

        public BasicBlock Block { get; }
        public int Index { get; }
        public bool AtOperation => Index < Block.Operations.Length;
        public IOperation Operation => Block.Operations[Index];
        public FlowPoint Next => new(Block, Index + 1);
        public bool Equals(FlowPoint other) => Block.Equals(other.Block) && Index == other.Index;
        public override bool Equals(object obj) => obj is FlowPoint other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Block.GetHashCode() * 397) ^ Index;
            }
        }

        public override string ToString()
        {
            var suffix = AtOperation
                ? $": {Operation.Syntax}"
                : "";
            return $"{Block.Ordinal}-{Index}{suffix}";
        }
    }
}