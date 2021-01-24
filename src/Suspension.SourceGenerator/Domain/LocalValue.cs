﻿using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class LocalValue : Value
    {
        private readonly ILocalSymbol local;

        public LocalValue(ILocalSymbol local)
        {
            this.local = local;
        }

        private bool Equals(LocalValue other) =>
            SymbolEqualityComparer.Default.Equals(local, other.local);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || obj is LocalValue other && Equals(other);

        public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(local);
        public override string ToString() => $"local: {local.Name}";
    }
}