using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Suspension.SourceGenerator.Domain;

namespace Suspension.SourceGenerator
{
    internal sealed class Graph2 : IEnumerable<(string From, string To, Scope References)>
    {
        private readonly ControlFlowGraph graph;

        public Graph2(ControlFlowGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerator<(string From, string To, Scope References)> GetEnumerator()
        {
            var visited = new HashSet<BasicBlock>();
            var queue = new Queue<Flow>();
            var entryFlow = EntryFlow;
            queue.Enqueue(entryFlow);
            var scopes = new Dictionary<FlowPoint, Scope>
            {
                { entryFlow.Reminder.Start, entryFlow.References }
            };

            while (queue.Count > 0)
            {
                m1:
                var flow = queue.Dequeue();
                var references = flow.References;
                var reminder = flow.Reminder;
                while (!reminder.Empty)
                {
                    var operation = reminder.Current;
                    if (operation.Accept(new SuspensionPoint.Is()))
                    {
                        var name = operation.Accept(new SuspensionPoint.Name());
                        yield return (flow.Origin, name, references);

                        queue.Enqueue(new Flow(name, reminder.Next(), ConstantScope.Empty));
                        goto m1;
                    }

                    var visitor = new ScopeVisitor();
                    references = operation.Accept(visitor, references);

                    reminder = reminder.Next();
                }

                foreach (var range in reminder.Continuations())
                {
                    queue.Enqueue(new Flow(flow.Origin, range, references));
                }
            }
        }

        private Flow EntryFlow => new Flow(
            "Entry",
            new BasicBlockRange(graph.Entry()),
            ConstantScope.Empty
        );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly struct Flow
        {
            public Flow(string origin, BasicBlockRange reminder, Scope references)
            {
                Origin = origin;
                Reminder = reminder;
                References = references;
            }

            public string Origin { get; }
            public BasicBlockRange Reminder { get; }
            public Scope References { get; }
        }
    }


    internal readonly struct FlowPoint
    {
        public FlowPoint(BasicBlock block, int index)
        {
            this.block = block;
            this.index = index;
        }

        private readonly BasicBlock block;
        private readonly int index;
    }

    internal readonly struct BasicBlockRange
    {
        private BasicBlockRange(BasicBlock block, int fromInclusive, int toExclusive)
        {
            this.Block = block;
            this.fromInclusive = fromInclusive;
            this.toExclusive = toExclusive;
        }

        public BasicBlockRange(BasicBlock block)
            : this(block, 0, block.Operations.Length)
        {
        }

        public BasicBlock Block { get; }
        private readonly int fromInclusive;
        private readonly int toExclusive;

        public bool Empty => fromInclusive >= toExclusive;

        public IOperation Current => Empty
            ? throw new InvalidOperationException()
            : Block.Operations[fromInclusive];

        public FlowPoint Start => new FlowPoint(Block, fromInclusive);

        public BasicBlockRange Next()
        {
            if (Empty)
                throw new InvalidOperationException();

            return new BasicBlockRange(Block, fromInclusive + 1, toExclusive);
        }

        public IEnumerable<BasicBlockRange> Continuations()
        {
            if (Block.FallThroughSuccessor is {} fallThrough)
            {
                yield return new BasicBlockRange(fallThrough.Destination);
            }

            if (Block.ConditionalSuccessor is {} conditional)
            {
                yield return new BasicBlockRange(conditional.Destination);
            }
        }
    }
}