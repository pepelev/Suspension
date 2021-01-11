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
                    private readonly System.Func<bool> argument;
                    private readonly System.Action<string> action;

                    public Start(System.Func<bool> argument, System.Action<string> action)
                    {
                        this.action = action;
                        this.argument = argument;
                    }

                    public override bool Completed => false;
                    public override None Result => throw new System.InvalidOperationException();

                    public override Coroutine<None> Run()
                    {
                        if (this.argument())
                        {
                            this.action("Hello");
                        }

                        this.action("World");
                        return new Finish();
                    }
                }
            }
        }
    }
}