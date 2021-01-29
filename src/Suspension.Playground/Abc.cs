using System;
using NUnit.Framework;

namespace Suspension.Playground
{
    public class Abc
    {
        [Test]
        public void Test()
        {
            Coroutine<None> entry = new SimpleFlow.Coroutines.Execute.Entry(Console.WriteLine);
            var first = entry.Run();
            var second = first.Run();
            var exit = second.Run();

            first.Run();

            while (!entry.Completed)
            {
                entry = entry.Run();
            }
        }
    }
}