using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Suspension.Tests.Samples;

namespace Suspension.Tests
{
    [TestFixtureSource(typeof(Cases))]
    public sealed class CoroutineShould
    {
        private readonly int count;
        private readonly Coroutine start;

        public CoroutineShould(Coroutine start, int count)
        {
            this.start = start;
            this.count = count;
        }

        [Test]
        public void There_Is_Single_Completed_Coroutine()
        {
            Run(start).Should().ContainSingle(coroutine => coroutine.Completed);
        }

        [Test]
        public void Throw_On_Run_When_Completed()
        {
            foreach (var coroutine in Run(start).Where(coroutine => coroutine.Completed))
            {
                Assert.Throws<InvalidOperationException>(
                    () => coroutine.Run()
                );
            }
        }

        [Test]
        public void Not_Throw_On_Second_Run()
        {
            foreach (var coroutine in Run(start).Where(coroutine => !coroutine.Completed))
            {
                Assert.DoesNotThrow(
                    () => coroutine.Run()
                );
            }
        }

        [Test]
        public void Have_Steps()
        {
            Run(start).Should().HaveCount(count);
        }

        private static IEnumerable<Coroutine> Run(Coroutine coroutine)
        {
            while (!coroutine.Completed)
            {
                yield return coroutine;
                coroutine = coroutine.Run();
            }

            yield return coroutine;
        }

        public sealed class Cases : IEnumerable<TestFixtureData>
        {
            public IEnumerator<TestFixtureData> GetEnumerator()
            {
                yield return new TestFixtureData(
                    new JustSingleLineOfCode.Coroutines.Execute.Entry(() => { }),
                    2
                );
                yield return new TestFixtureData(
                    new SimpleSuspensionPoint.Coroutines.Execute.Entry(_ => { }),
                    3
                );
                yield return new TestFixtureData(
                    new Countdown.Coroutines.Report.Entry(6, _ => { }),
                    8
                );
                yield return new TestFixtureData(
                    new SimpleIf.Coroutines.Execute.Entry(() => true, _ => { }),
                    4
                );
                yield return new TestFixtureData(
                    new SimpleIf.Coroutines.Execute.Entry(() => false, _ => { }),
                    3
                );

                var cases = new[]
                {
                    Call,
                    New,
                    DiscardAssignment,
                    Parameters
                }.SelectMany(x => x);

                foreach (var (coroutine, count) in cases)
                {
                    yield return new TestFixtureData(coroutine, count);
                }
            }

            private static IEnumerable<(Coroutine Coroutine, int count)> Call =>
                new (Coroutine Coroutine, int count)[]
                {
                    (new Call.Coroutines.StaticMethod.Entry(), 2),
                    (new Call.Coroutines.InstanceMethod.Entry("42"), 2),
                    (new Call.Coroutines.Delegate.Entry(() => { }), 2),
                    (new Call.Coroutines.DelegateWithParameter.Entry(_ => { }), 2),
                    (new Call.Coroutines.InvokeDelegate.Entry(() => { }), 2),
                    (new Call.Coroutines.InvokeDelegateWithParameter.Entry(_ => { }), 2),
                    (new Call.Coroutines.PrivateStaticMethod.Entry(), 2)
                };

            private static IEnumerable<(Coroutine Coroutine, int count)> New =>
                new (Coroutine Coroutine, int count)[]
                {
                    (new New.Coroutines.String.Entry(), 2),
                    (new New.Coroutines.Object.Entry(), 2),
                    (new New.Coroutines.NullableInt.Entry(), 2),
                    (new New.Coroutines.Int.Entry(), 2)
                };

            private static IEnumerable<(Coroutine Coroutine, int count)> DiscardAssignment =>
                new (Coroutine Coroutine, int count)[]
                {
                    (new DiscardAssignment.Coroutines.Variable.Entry(), 2),
                    (new DiscardAssignment.Coroutines.OutParameter.Entry(), 2)
                };

            private static IEnumerable<(Coroutine Coroutine, int count)> Parameters =>
                new (Coroutine Coroutine, int count)[]
                {
                    (new Parameters.Coroutines.Regular.Entry("str"), 2),
                    (new Parameters.Coroutines.Out.Entry(_ => { }), 3),
                    (new Parameters.Coroutines.OutDiscard.Entry(), 2),
                    (new Parameters.Coroutines.OutDeclaration.Entry(_ => { }), 3)
                };

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}