using System;
using NUnit.Framework;

namespace Suspension.Playground
{
    public class Abc
    {
        [Test]
        public void Test()
        {
            var entry = new SimpleWhile.Coroutines.Execute.Entry(Console.WriteLine, () => false);
            entry.Run().Run();
        }
    }
}