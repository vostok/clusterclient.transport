using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class EmptyContent_Tests
    {
        private EmptyContent content;

        [SetUp]
        public void TestSetup()
        {
            content = new EmptyContent();
        }

        [Test]
        public void Should_have_zero_content_length()
        {
            content.Headers.ContentLength.Should().Be(0L);
        }

        [Test]
        public void Should_return_no_data()
        {
            content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }
    }
}
