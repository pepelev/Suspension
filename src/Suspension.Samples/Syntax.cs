using System;

namespace Suspension.Samples
{
    public sealed partial class Syntax
    {
        private static void MarkUsed(object a)
        {
        }

        private static void RefParameter(ref object a)
        {
        }

        private static void OutParameter(out object a)
        {
            a = "";
        }

        private static void InParameter(in object a)
        {
        }

        private static void NamedParameters(string a, int b)
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
        public static void Literal()
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
#if MethodCall_StaticMethod
            var staticMethod = string.Join("|", "a"); 
#endif
#if MethodCall_InstanceMethod
            MarkUsed("1234567890".Substring(1)); 
#endif
#if MethodCall_RefParameter
            object refParameter = 40;
            RefParameter(ref refParameter);
#endif
#if MethodCall_OutParameter
            object outParameter;
            OutParameter(out outParameter);
#endif
#if MethodCall_OutVarParameter
            OutParameter(out var outParameter);
#endif
#if MethodCall_InParameter
            object inParameter = 40;
            InParameter(in inParameter);
#endif
#if MethodCall_NamedParameters
            NamedParameters(b: 42, a: "hello");
#endif
        }
#endif

#if ParameterReference
        [Suspendable]
        public static void ParameterReference(int a)
        {
            MarkUsed(a);
        }
#endif

#if ArrayElementReference
        [Suspendable]
        public static void ArrayElementReference(int[] a, int[][] b, int[,] c)
        {
#if ArrayElementReference_Regular
            MarkUsed(a[5]);
#endif
#if ArrayElementReference_ArrayOfArrays
            MarkUsed(b[5][6]);
#endif
#if ArrayElementReference_TwoDimensionalArray
            MarkUsed(c[5, 6]);
#endif
        }
#endif

#if Assignment
        [Suspendable]
        public static void Assignment(int a)
        {
#if Assignment_Regular
            int regular;
            regular = a;
#endif
#if Assignment_Compound
            int first;
            int second;
            first = second = a;
#endif
#if Assignment_Discard
            _ = a;
#endif

            //todo deny ref locals
#if Assignment_Ref
            ref var refVariable = ref a;
#endif
#if Assignment_Deconstruction
            int left;
            string right;
            (left, right) = (10, "cat");
#endif
#if Assignment_DeconstructionDeclaration
            var (left, right) = (10, "cat");
#endif
        }
#endif
    }
}