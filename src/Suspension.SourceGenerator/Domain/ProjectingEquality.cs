using System;
using System.Collections.Generic;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class ProjectingEquality<Value, Key> : IEqualityComparer<Value>
    {
        private readonly Func<Value, Key> projection;
        private readonly IEqualityComparer<Key> equality;

        public ProjectingEquality(Func<Value, Key> projection, IEqualityComparer<Key> equality)
        {
            this.projection = projection;
            this.equality = equality;
        }

        public bool Equals(Value x, Value y) => equality.Equals(
            projection(x),
            projection(y)
        );

        public int GetHashCode(Value obj) => equality.GetHashCode(
            projection(obj)
        );
    }
}