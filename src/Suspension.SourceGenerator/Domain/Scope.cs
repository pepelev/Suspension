using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Suspension.SourceGenerator.Domain.Values;

namespace Suspension.SourceGenerator.Domain
{
    internal abstract class Scope : IEnumerable<Value>
    {
        public abstract Scope Add(Value value);
        public abstract IEnumerator<Value> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public abstract Value Find(Value target);

        public Scope Union(IEnumerable<Value> values) => new ConstantScope(
            this.AsEnumerable().Union(values)
        );

        public Scope Except(IEnumerable<Value> values) => new ConstantScope(
            this.AsEnumerable().Except(values)
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