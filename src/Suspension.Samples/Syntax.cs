using System;

namespace Suspension.Samples
{
    public sealed partial class Syntax
    {
        private static void MarkUsed(object a)
        {
        }

        [Suspendable]
        public static void IntPlus(int a, int b, Action<int> report)
        {
            report(-13 + a);
        }

#if ObjectCreation
        [Suspendable]
        public static void ObjectCreation()
        {
#if ObjectCreation_Object
            MarkUsed(new object());
#endif
#if ObjectCreation_String
            MarkUsed(new string('c', 17));
#endif
#if ObjectCreation_ArrayOfStrings
            MarkUsed(new string[17]);
#endif
#if ObjectCreation_ArrayOfStringsWithInitializer
            MarkUsed(new[] {"Hello", "World"});
#endif
#if ObjectCreation_MultidimensionalArrayOfStrings
            MarkUsed(new string[17, 3, 7]);
#endif
#if ObjectCreation_ListOfStrings
            MarkUsed(new System.Collections.Generic.List<string>());
#endif
        }
#endif

#if KeywordVariableName
        [Suspendable]
        public static void KeywordVariableName()
        {
            var @int = 72;
            MarkUsed(@int);
        }
#endif

#if Literal
        [Suspendable]
        public static void Literals()
        {
#if Literal_Int
            MarkUsed(-42);
#endif
#if Literal_UInt
            MarkUsed(93U);
#endif
#if Literal_Long
            MarkUsed(10L);
#endif
#if Literal_ULong
            MarkUsed(10UL);
#endif
#if Literal_String
            MarkUsed("hello");
#endif
#if Literal_Char
            MarkUsed('a');
#endif
#if Literal_Float
            MarkUsed(12.9f);
#endif
#if Literal_Double
            MarkUsed(12.9);
#endif
#if Literal_Decimal
            MarkUsed(12.9m);
#endif
        }
#endif

#if MethodCall
        [Suspendable]
        public static void MethodCall()
        {
            var staticMethod = string.Join("|", "a");
        }
#endif
    }
}