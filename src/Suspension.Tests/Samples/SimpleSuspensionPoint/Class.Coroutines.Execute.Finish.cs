namespace Suspension.Tests.Samples.SimpleSuspensionPoint
{
    public static partial class Class
    {
        public static partial class Coroutines
        {
            public static partial class Execute
            {
                public sealed class Finish : Coroutine<None>
                {
                    public override bool Completed => true;
                    public override None Result => new None();

                    public override Coroutine<None> Run()
                    {
                        throw new System.InvalidOperationException();
                    }
                }
            }
        }
    }
}