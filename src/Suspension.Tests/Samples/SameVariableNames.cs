using System;

namespace Suspension.Tests.Samples
{
    public sealed partial class SameVariableNames
    {
        //[Suspendable] todo uncomment
        public static void SameType()
        {
            {
                var a = 10;
                Console.WriteLine(a);
            }
            {
                var a = 12;
                Console.WriteLine(a);
            }
        }

        //[Suspendable] todo uncomment
        public static void DifferentTypes()
        {
            {
                var a = 10;
                Console.WriteLine(a);
            }
            {
                var a = "12";
                Console.WriteLine(a);
            }
        }
    }
}