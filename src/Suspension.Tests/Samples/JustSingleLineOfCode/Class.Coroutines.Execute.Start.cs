namespace Suspension.Tests.Samples.JustSingleLineOfCode
{
    public static partial class Class
    {
        public static partial class Coroutines
        {
            public static partial class Execute
            {
                public sealed class Start : Coroutine<None>
                {
private readonly System.Action action;
                    public Start(System.Action action)
                    {
this.action = action;
                    }

                    public override bool Completed => false;
                    public override None Result => throw new System.InvalidOperationException();

                    public override Coroutine<None> Run()
                    {
                        action();
                        return new Finish();
                    }
                }
            }
        }
    }
}