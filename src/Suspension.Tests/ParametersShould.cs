using NUnit.Framework;
using Suspension.Tests.Fixtures;
using static Suspension.Samples.Parameters.Coroutines;
using static Suspension.Tests.Fixtures.Plan.Stage;

namespace Suspension.Tests
{
    public sealed class ParametersShould
    {
        [Test]
        [TestCase("str", true)]
        [TestCase("no-str", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void UseArguments(string argument, bool equality)
        {
            var plan = new Plan(
                Invoke("equals", equality)
            );

            var coroutine = new Regular.Entry(argument, plan.Spy<bool>("equals"));
            plan.AssertMatch(coroutine);
        }

        [Test]
        public void WorkWithOut()
        {
            var plan = new Plan(
                Suspend("Middle"),
                Invoke("action", 123)
            );

            var coroutine = new Out.Entry(plan.Spy<int>("action"));
            plan.AssertMatch(coroutine);
        }

        [Test]
        public void WorkWithOutDeclaration()
        {
            var plan = new Plan(
                Invoke("action", 19),
                Suspend("Parsed"),
                Invoke("action", 26)
            );

            var coroutine = new OutDeclaration.Entry(plan.Spy<int>("action"));
            plan.AssertMatch(coroutine);
        }
    }
}