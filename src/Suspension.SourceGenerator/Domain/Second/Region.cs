using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Generator;

namespace Suspension.SourceGenerator.Domain.Second
{
    internal abstract class Region : IEnumerable<StatementSyntax>
    {
        public abstract IEnumerator<StatementSyntax> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class RegularRegion : Region
    {
        private readonly ControlFlowGraph graph;
        private readonly ControlFlowRegion region;
        private readonly Scope scope;
        private readonly FlowPoint start;

        public override IEnumerator<StatementSyntax> GetEnumerator()
        {
            if (StartInside)
            {

            }

            yield break;
        }

        private bool StartInside => start.Block.Ordinal.Between(region.FirstBlockOrdinal, region.LastBlockOrdinal);
    }
}