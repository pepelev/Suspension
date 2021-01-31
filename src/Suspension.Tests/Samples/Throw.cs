using System;
using FluentAssertions;
using NUnit.Framework;

namespace Suspension.Tests.Samples
{
    public partial class Throw
    {
        [Suspendable]
        public static void Execute()
        {
            throw new Exception("Boom");
        }

        [Test]
        public void Should_Throw_Exception()
        {
            var entry = new Coroutines.Execute.Entry();

            var exception = Assert.Throws<Exception>(
                () => entry.Run()
            );
            exception.Message.Should().Be("Boom");
            exception.StackTrace.Should().MatchRegex(
                @"^   at Suspension.Tests.Samples.Throw.Coroutines.Execute.Entry.Run\(\)" +
                @" in .*\\Suspension\\src\\Suspension.Tests\\Samples\\Throw.cs:line 12"
            );
        }
    }
}