using System;

namespace Suspension.Tests.Samples.Manual.Throw
{
    public class SuspensionPointInsideTryAndWhile2
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
                int b;
                var variableA = a;

                @while:
                try
                {
                    b = 30;
                    Console.WriteLine("Hello");
                    if (variableA < 17_358)
                    {
                        goto Inside;
                    }

                    variableA += b;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    goto exit;
                }

                Console.WriteLine("Outside");
                goto @while;

                Inside:
                return new Inside(variableA, b);

                exit:
                Console.WriteLine(variableA);
                return new Exit();
            }
        }

        public class Inside
        {
            private readonly int a;
            private readonly int b;

            public Inside(int a, int b)
            {
                this.a = a;
                this.b = b;
            }

            public object Run()
            {
                var variableB = b;
                var variableA = a;

                var __entering = true;

                @while:
                try
                {
                    if (__entering)
                    {
                        goto start;
                    }

                    variableB = 30;
                    Console.WriteLine("Hello");
                    if (variableA < 17_358)
                    {
                        goto Inside;
                    }

                    start:
                    __entering = false;
                    variableA += variableB;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    goto exit;
                }

                Console.WriteLine("Outside");
                goto @while;

                Inside:
                return new Inside(variableA, variableB);

                exit:
                Console.WriteLine(variableA);
                return new Exit();
            }
        }

        public class Exit
        {
        }
    }
}