using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Tests.Universal
{
    [TestFixture]
    internal class UniversalTransport_Tests
    {
        private UniversalTransport transport;

        [SetUp]
        public void TestSetup()
        {
            transport = new UniversalTransport(new SilentLog());
        }

        [Test]
        public void Capabilities_property_should_work_before_first_request_is_made()
        {
            transport.Capabilities.Should().NotBe(TransportCapabilities.None);
        }
    }
}