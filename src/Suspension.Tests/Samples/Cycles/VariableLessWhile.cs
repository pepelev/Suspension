using System;

namespace Suspension.Tests.Samples.Cycles
{
    public partial class VariableLessWhile
    {
        [Suspendable]
        public static void Execute(Func<bool> argument, Action<string> action)
        {
            while (argument())
            {
                Flow.Suspend("InsideWhile");
                action("Hello");
                while (argument())
                {
                    action("visited check");
                }
            }

            Flow.Suspend("OutsideWhile");
            action("World");
        }
    }
}


namespace Suspension.Tests.Samples.Cycles
{
    public static partial class VariableLessWhile
    {
        public static partial class Coroutines
        {
            public abstract partial class Execute : Suspension.Coroutine<Suspension.None>
            {
                public sealed class Entry : global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute
                {
                    private readonly global::System.Action<global::System.String> action;
                    private readonly global::System.Func<global::System.Boolean> argument;
                    public Entry(global::System.Action<global::System.String> action, global::System.Func<global::System.Boolean> argument)
                    {
                        this.action = action;
                        this.argument = argument;
                    }

                    public override System.Boolean Completed => false;
                    public override Suspension.None Result => throw new System.InvalidOperationException();
                    public override Suspension.Coroutine<Suspension.None> Run()
                    {
                        block0:
                        ;
                        goto block1;
                        block1:
                        ;
                        if (!this.argument())
                            goto block5;
                        goto block2;
                        block5:
                        ;
                        return new global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute.OutsideWhile(this.action);
                        block2:
                        ;
                        return new global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute.InsideWhile(this.action, this.argument);
                    }
                }
            }
        }
    }
}

namespace Suspension.Tests.Samples.Cycles
{
    public static partial class VariableLessWhile
    {
        public static partial class Coroutines
        {
            public abstract partial class Execute : Suspension.Coroutine<Suspension.None>
            {
                public sealed class OutsideWhile : global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute
                {
                    private readonly global::System.Action<global::System.String> action;
                    public OutsideWhile(global::System.Action<global::System.String> action)
                    {
                        this.action = action;
                    }

                    public override System.Boolean Completed => false;
                    public override Suspension.None Result => throw new System.InvalidOperationException();
                    public override Suspension.Coroutine<Suspension.None> Run()
                    {
                        block5:
                        ;
                        this.action("World");
                        goto block6;
                        block6:
                        ;
                        return new global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute.Exit();
                    }
                }
            }
        }
    }
}

namespace Suspension.Tests.Samples.Cycles
{
    public static partial class VariableLessWhile
    {
        public static partial class Coroutines
        {
            public abstract partial class Execute : Suspension.Coroutine<Suspension.None>
            {
                public sealed class InsideWhile : global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute
                {
                    private readonly global::System.Action<global::System.String> action;
                    private readonly global::System.Func<global::System.Boolean> argument;
                    public InsideWhile(global::System.Action<global::System.String> action, global::System.Func<global::System.Boolean> argument)
                    {
                        this.action = action;
                        this.argument = argument;
                    }

                    public override System.Boolean Completed => false;
                    public override Suspension.None Result => throw new System.InvalidOperationException();
                    public override Suspension.Coroutine<Suspension.None> Run()
                    {
                        block2:
                        ;
                        this.action("Hello");
                        goto block3;
                        block3:
                        ;
                        if (!this.argument())
                            goto block1;
                        goto block4;
                        block1:
                        ;
                        if (!this.argument())
                            goto block5;
                        goto block2;
                        block4:
                        ;
                        this.action("visited check");
                        goto block3;
                        block5:
                        ;
                        return new global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute.OutsideWhile(this.action);
                    }
                }
            }
        }
    }
}

namespace Suspension.Tests.Samples.Cycles
{
    public static partial class VariableLessWhile
    {
        public static partial class Coroutines
        {
            public abstract partial class Execute : Suspension.Coroutine<Suspension.None>
            {
                public sealed class Exit : global::Suspension.Tests.Samples.Cycles.VariableLessWhile.Coroutines.Execute
                {
                    public override System.Boolean Completed => true;
                    public override Suspension.None Result => new Suspension.None();
                    public override Suspension.Coroutine<Suspension.None> Run() => throw new System.InvalidOperationException();
                }
            }
        }
    }
}
