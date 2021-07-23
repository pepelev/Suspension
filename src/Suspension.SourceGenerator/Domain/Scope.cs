using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Domain
{
    internal abstract class Scope : IEnumerable<Value>
    {
        private static readonly ProjectingEquality<Value, Value.Identity> equality = new(
            value => value.Id,
            EqualityComparer<Value.Identity>.Default
        );
        public abstract Scope Union(Value value);
        public abstract IEnumerator<Value> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public abstract bool Contains(Value.Identity target);
        public abstract Value Find(Value.Identity target);

        public Scope Union(IEnumerable<Value> values) => new ConstantScope(
            this.AsEnumerable().Union(values, equality)
        );

        // todo equality for values does not make sense
        public Scope Except(IEnumerable<Value> values) => new ConstantScope(
            this.AsEnumerable().Except(values, equality)
        );

        public override string ToString()
        {
            if (!this.Any())
            {
                return ":Empty:";
            }

            return string.Join(", ", this);
        }
    }
}