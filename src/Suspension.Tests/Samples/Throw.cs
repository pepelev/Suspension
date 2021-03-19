using System;
using NUnit.Framework;

//using FluentAssertions;
//using NUnit.Framework;

namespace Suspension.Tests.Samples
{
    public partial class Throw
    {
        //[Suspendable]
        public static void SimpleThrow()
        {
            throw new Exception("Boom");
        }

        public static void ThrowFromIf(int a)
        {
            {
                var b = 30;
                if (a < 42)
                    throw new Exception("inside if");

                a += b;
            }

            Console.WriteLine(a);
        }

        public static void SuspensionPointInsideTryAndWhile(int a)
        {
            while (true)
            {
                try
                {
                    var b = 30;
                    Console.WriteLine("Hello");
                    Flow.Suspend("Inside");
                    a += b;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }

            Console.WriteLine(a);
        }

        public static void SuspensionPointInsideTryAndWhile2(int a)
        {
            while (true)
            {
                try
                {
                    var b = 30;
                    Console.WriteLine("Hello");
                    if (a < 17_358)
                        Flow.Suspend("Inside");

                    a += b;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }

                Console.WriteLine("Outside");
            }

            Console.WriteLine(a);
        }

        public static void TryCatchException(int a)
        {
            try
            {
                Console.WriteLine(a);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TryCatch(int a)
        {
            try
            {
                Console.WriteLine(a);
                Flow.Suspend("Try");
                Console.WriteLine(a + 1);
            }
            catch
            {
                Console.WriteLine();
                Flow.Suspend("Catch");
                Console.WriteLine();
            }
        }

        //[Test]
        public static void TryCatchFinally()
        {
            try
            {
                Console.WriteLine("t-before");
                Flow.Suspend("Try");
                Console.WriteLine("t-after");
            }
            catch
            {
                Console.WriteLine("c-before");
                Flow.Suspend("Catch");
                Console.WriteLine("c-after");
            }
            finally
            {
                Console.WriteLine("f-before");
                Flow.Suspend("Finally");
                Console.WriteLine("f-after");
            }
        }

        public static void TryTwoCatch(int a)
        {
            try
            {
                Console.WriteLine(a);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TryCatchWhen(int a)
        {
            try
            {
                Console.WriteLine(a);
            }
            catch (Exception e) when (e.Message[0] > 48)
            {
                Console.WriteLine(e);
            }
        }

        private sealed class A : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public static void Using(int a)
        {
            using (new A())
            {
                Console.WriteLine(a);
            }
        }

        public static void UsingNull(int a)
        {
            using (null)
            {
                Console.WriteLine(a);
            }
        }

        public static void TwoTry(int a)
        {
            try
            {
                var p = "asdad".GetHashCode();
                try
                {
                    Console.WriteLine(a);
                    if (a < 40)
                    {
                        Flow.Suspend("ALess40");
                    }

                    Console.WriteLine(a + 7);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TryTwoCatchWhen(int a)
        {
            try
            {
                Console.WriteLine(a);
                if (a < 40)
                {
                    Flow.Suspend("ALess40");
                }

                Console.WriteLine(a + 7);
            }
            catch (ArgumentException e) when (e.Message[0] > 48)
            {
                Flow.Suspend("Argument");
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TryTwoCatchWhenEntry(int a)
        {
            try
            {
                Console.WriteLine(a);
                if (a < 40)
                {
                    TryTwoCatchWhenALess40(a);
                    return;
                }

                Console.WriteLine(a + 7);
            }
            catch (ArgumentException e) when (e.Message[0] > 48)
            {
                TryTwoCatchWhenArgument(e);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TryTwoCatchWhenALess40(int a)
        {
            try
            {
                Console.WriteLine(a + 7);
            }
            catch (ArgumentException e) when (e.Message[0] > 48)
            {
                Flow.Suspend("Argument");
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TryTwoCatchWhenArgument(Exception e)
        {
            Console.WriteLine(e);
        }

        //[Test]
        //public void Should_Throw_Exception()
        //{
        //    var entry = new Coroutines.SimpleThrow.Entry();

        //    var exception = Assert.Throws<Exception>(
        //        () => entry.Run()
        //    );
        //    exception.Message.Should().Be("Boom");
        //    exception.StackTrace.Should().MatchRegex(
        //        @"^   at Suspension.Tests.Samples.Throw.Coroutines.Execute.Entry.Run\(\)" +
        //        @" in .*\\Suspension\\src\\Suspension.Tests\\Samples\\Throw.cs:line 12"
        //    );
        //}
    }
}