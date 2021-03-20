using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    internal static class RegionExtensions
    {
        public static IEnumerable<ControlFlowRegion> Containing(this ControlFlowRegion start, ControlFlowRegionKind target) =>
            start.Hierarchy().Where(region => region.Kind == target);

        public static IEnumerable<ControlFlowRegion> Hierarchy(this ControlFlowRegion start)
        {
            for (var region = start; region != null; region = region.EnclosingRegion)
            {
                yield return region;
            }
        }
    }
}