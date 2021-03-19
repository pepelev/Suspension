using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Domain
{
    internal sealed class ConstantScope : Scope
    {
        private readonly ImmutableHashSet<Value> uniqueness;
        private readonly ImmutableSortedDictionary<int, Value> order;

        private ConstantScope((ImmutableHashSet<Value> Uniqueness, ImmutableSortedDictionary<int, Value> Order) Pair)
        {
            (uniqueness, order) = Pair;
        }

        public ConstantScope(IEnumerable<Value> values)
            : this(Build(values))
        {
        }

        private static (ImmutableHashSet<Value> Uniqueness, ImmutableSortedDictionary<int, Value> Order) Build(IEnumerable<Value> values)
        {
            var uniquenessBuilder = ImmutableHashSet.CreateBuilder<Value>();
            var orderBuilder = ImmutableSortedDictionary.CreateBuilder<int, Value>();
            foreach (var value in values)
            {
                if (!uniquenessBuilder.Add(value))
                {
                    if (!Debugger.IsAttached)
                    {
                        Debugger.Launch();
                    }
                    throw new ArgumentException("Contains duplicates", nameof(values));
                }

                orderBuilder.Add(orderBuilder.Count, value);
            }

            return (uniquenessBuilder.ToImmutable(), orderBuilder.ToImmutable());
        }

        public static Scope Empty { get; } = new ConstantScope(Array.Empty<Value>());

        public override Scope Add(Value value)
        {
            if (uniqueness.Contains(value))
            {
                return this;
            }

            return new ConstantScope(
                (
                    uniqueness.Add(value),
                    order.Add(order.Count, value)
                )
            );
        }

        public override IEnumerator<Value> GetEnumerator() => order.Values.GetEnumerator();
        public override Value Find(Value target) => uniqueness.Single(value => value.Equals(target));
    }
}