namespace Suspension.Tests.Samples.SimpleSuspensionPoint
{
    public static partial class Class
    {
        public static partial class Coroutines
        {
            public static partial class Execute
            {
                public sealed class Middle : Coroutine<None>
                {
                    private readonly System.Action<string> action;

                    public Middle(System.Action<string> action)
                    {
                        this.action = action;
                    }

                    public override bool Completed => false;
                    public override None Result => throw new System.InvalidOperationException();

                    public override Coroutine<None> Run()
                    {
                        this.action("World");
                        return new SimpleSuspensionPoint.Class.Coroutines.Execute.Finish();
                    }
                }
            }
        }
    }
}