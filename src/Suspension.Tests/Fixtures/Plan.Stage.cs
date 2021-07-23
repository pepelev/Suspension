namespace Suspension.Tests.Fixtures
{
    public sealed partial class Plan
    {
        public abstract class Stage
        {
            public abstract bool IncludedBy(Stage stage);
            public abstract bool Includes<T>(Invocation<T> invocation);
            public abstract bool Includes(Suspension suspension);

            public static Stage Suspend(string name) => new Suspension(name);
            public static Stage Invoke<T>(string name, T argument) => new Invocation<T>(name, argument);

            public sealed class Suspension : Stage
            {
                private readonly string name;

                public Suspension(string name)
                {
                    this.name = name;
                }

                public override string ToString() => $"Suspension[{name}]";
                public override bool IncludedBy(Stage stage) => stage.Includes(this);
                public override bool Includes<T>(Invocation<T> invocation) => false;
                public override bool Includes(Suspension suspension) => suspension.name == name;
            }

            public sealed class Invocation<T> : Stage
            {
                private readonly string spyName;
                private readonly T expectation;

                public Invocation(string spyName, T expectation)
                {
                    this.spyName = spyName;
                    this.expectation = expectation;
                }

                public override string ToString() => $"{spyName}({expectation})";

                public override bool IncludedBy(Stage stage) => stage.Includes(this);

                public override bool Includes<TArg>(Invocation<TArg> invocation) => Equals(
                    (spyName, expectation),
                    (invocation.spyName, invocation.expectation)
                );

                public override bool Includes(Suspension suspension) => false;
            }
        }
    }
}