using System;
using NUnit.Framework;

namespace Suspension.Tests.Samples
{
    public partial class Field
    {
        private int count;

        public Field(int count)
        {
            this.count = count;
        }

        [Suspendable]
        public void Go()
        {
            while (count > 0)
            {
                count--;
                Console.WriteLine(count);
                Flow.Suspend("Inside");
            }
        }
    }

    public sealed class FieldShould
    {
        [Test]
        public void Test()
        {
            Coroutine<None> coroutine = new Field.Coroutines.Go.Entry(7);
            while (!coroutine.Completed)
            {
                coroutine = coroutine.Run();
            }
        }
    }
}