using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class ConstantScope : Scope
    {
        private readonly ImmutableDictionary<Value.Identity, Value> uniqueness;
        private readonly ImmutableSortedDictionary<int, Value> order;

        private ConstantScope((ImmutableDictionary<Value.Identity, Value> Uniqueness, ImmutableSortedDictionary<int, Value> Order) Pair)
        {
            (uniqueness, order) = Pair;
        }

        public ConstantScope(IEnumerable<Value> values)
            : this(Build(values))
        {
        }

        private static (ImmutableDictionary<Value.Identity, Value> Uniqueness, ImmutableSortedDictionary<int, Value> Order) Build(IEnumerable<Value> values)
        {
            var uniquenessBuilder = ImmutableDictionary.CreateBuilder<Value.Identity, Value>();
            var orderBuilder = ImmutableSortedDictionary.CreateBuilder<int, Value>();
            foreach (var value in values)
            {
                if (uniquenessBuilder.ContainsKey(value.Id))
                {
                    throw new ArgumentException("Contains duplicates", nameof(values));
                }

                uniquenessBuilder.Add(value.Id, value);
                orderBuilder.Add(orderBuilder.Count, value);
            }

            return (uniquenessBuilder.ToImmutable(), orderBuilder.ToImmutable());
        }

        public static Scope Empty { get; } = new ConstantScope(Array.Empty<Value>());

        public override Scope Union(Value value)
        {
            if (uniqueness.ContainsKey(value.Id))
            {
                return this;
            }

            return new ConstantScope(
                (
                    uniqueness.Add(value.Id, value),
                    order.Add(order.Count, value)
                )
            );
        }

        public override IEnumerator<Value> GetEnumerator() => order.Values.GetEnumerator();
        public override Value Find(Value.Identity target) => uniqueness[target];
    }
}