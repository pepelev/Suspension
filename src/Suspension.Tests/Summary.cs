using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Suspension.Tests
{
    public sealed class Summary<T> : IEnumerable<string>
    {
        private readonly IEnumerable<T> sequence;
        private readonly int maxEntries;

        public Summary(IEnumerable<T> sequence, int maxEntries = 4)
        {
            this.sequence = sequence;
            this.maxEntries = maxEntries;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var content = sequence.Take(maxEntries + 1).ToList();
            if (content.Count <= maxEntries)
            {
                return content.Select(item => $"{item}").GetEnumerator();
            }

            return content
                .Take(maxEntries - 1)
                .Select(item => $"{item}")
                .Append("...")
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}