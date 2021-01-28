using System;
using NUnit.Framework;

namespace Suspension.Playground
{
    public partial class SimpleWhile
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            while (argument())
            {
                Flow.Suspend("InsideWhile");
                action("Hello");
                while (argument())
                {
                    action("visited check");
                }
            }

            Flow.Suspend("OutsideWhile");
            action("World");
        }
    }

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