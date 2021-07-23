using System;
using System.Collections.Generic;
using System.Linq;

namespace Suspension.Tests.Fixtures
{
    public sealed partial class Plan
    {
        private readonly Queue<Stage> expectedStages;

        public Plan(params Stage[] expectedStages)
        {
            this.expectedStages = new Queue<Stage>(
                expectedStages.Append(
                    Stage.Suspend("Exit")
                )
            );
        }

        public void AssertMatch(Coroutine coroutine)
        {
            while (!coroutine.Completed)
            {
                coroutine = coroutine.Run();
                var name = coroutine.GetType().Name;
                var suspension = Stage.Suspend(name);
                AssertStage(suspension);
            }

            AssertNoMoreStages();
        }

        private void AssertStage(Stage actual)
        {
            if (expectedStages.Count == 0)
            {
                throw new Exception(
                    $"Expected no more stages, but '{actual}' occurred"
                );
            }

            var stage = expectedStages.Dequeue();
            if (!actual.IncludedBy(stage))
            {
                throw new Exception(
                    $"Expected '{stage}', but '{actual}' occurred"
                );
            }
        }

        private void AssertNoMoreStages()
        {
            if (expectedStages.Count > 0)
            {
                throw new Exception(
                    $"Expected {expectedStages.Count} more stages:\n" +
                    string.Join("\n", new Summary<Stage>(expectedStages))
                );
            }
        }

        public Action<T> Spy<T>(string name) => argument =>
        {
            var invoke = Stage.Invoke(name, argument);
            AssertStage(invoke);
        };
    }
}