using System;

namespace Suspension.Tests.Samples.Manual.Throw
{
    public class TryCatch
    {
        public class Entry
        {
            private readonly int a;

            public Entry(int a)
            {
                this.a = a;
            }

            public object Run()
            {
                try
                {
                    Console.WriteLine(a);
                }
                catch
                {
                    Console.WriteLine();
                    return new Catch();
                }

                return new Try(a);
            }
        }

        public class Try
        {
            private readonly int a;

            public Try(int a)
            {
                this.a = a;
            }

            public object Run()
            {
                try
                {
                    Console.WriteLine(a + 1);
                }
                catch
                {
                    Console.WriteLine();
                    return new Catch();
                }

                return new Exit();
            }
        }

        public class Catch
        {
            public object Run()
            {
                Console.WriteLine();
                return new Exit();
            }
        }

        public class Exit
        {
        }
    }
}