using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class LocalVariable : Value
    {
        private readonly ILocalSymbol local;

        public LocalVariable(ILocalSymbol local)
        {
            this.local = local;
        }

        private bool Equals(LocalVariable other) =>
            SymbolEqualityComparer.Default.Equals(local, other.local);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || obj is LocalVariable other && Equals(other);

        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(local);
    }
}