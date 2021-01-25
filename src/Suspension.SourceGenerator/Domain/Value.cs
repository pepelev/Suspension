using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Domain
{
    internal abstract class Value
    {
        public abstract ITypeSymbol Type { get; }
        public abstract string Name { get; }
    }
}