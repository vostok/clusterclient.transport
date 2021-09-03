using FluentAssertions;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests
{
    [TestFixture]
    public class Test_Tests
    {
        [Test]
        public void Good()
        {
            new SynchronousConsoleLog().Info("info");
            new SynchronousConsoleLog().Warn("warn");
            new SynchronousConsoleLog().Error("error");
        }
        
        [Test]
        public void Bad()
        {
            new SynchronousConsoleLog().Info("info");
            new SynchronousConsoleLog().Warn("warn");
            new SynchronousConsoleLog().Error("error");
            1.Should().Be(2);
        }
    }
}