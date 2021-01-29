using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class ConstantScope : Scope
    {
        private readonly ImmutableHashSet<Value> values;

        private ConstantScope(ImmutableHashSet<Value> values)
        {
            this.values = values;
        }

        public ConstantScope(IEnumerable<Value> values)
            : this(ImmutableHashSet<Value>.Empty.Union(values))
        {
        }

        public static Scope Empty { get; } = new ConstantScope(Array.Empty<Value>());

        public override Scope Add(Value value) => new ConstantScope(
            values.Add(value)
        );

        public override IEnumerator<Value> GetEnumerator() => values.GetEnumerator();

        public override Value Find(Value target) => values.Single(value => value.Equals(target));
    }
}