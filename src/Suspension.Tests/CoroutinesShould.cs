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
    public sealed class CoroutinesShould
    {
        private readonly int count;
        private readonly Coroutine start;

        public CoroutinesShould(Coroutine start, int count)
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
                    new SimpleIf.Coroutines.Execute.Entry(_ => { }, () => true),
                    4
                );
                yield return new TestFixtureData(
                    new SimpleIf.Coroutines.Execute.Entry(_ => { }, () => false),
                    3
                );
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}