using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Tests.Sockets
{
    [TestFixture]
    internal class SocketsTransport_Tests
    {
        private SocketsTransport transport;

        [SetUp]
        public void TestSetup()
            => transport = new SocketsTransport(new SilentLog());

        [Test]
        public void Should_advertise_request_composite_body_capability()
            => transport.Supports(TransportCapabilities.RequestCompositeBody).Should().BeTrue();

        [Test]
        public void Should_advertise_request_streaming_capability()
            => transport.Supports(TransportCapabilities.RequestStreaming).Should().BeTrue();

        [Test]
        public void Should_advertise_response_streaming_capability()
            => transport.Supports(TransportCapabilities.ResponseStreaming).Should().BeTrue();
        
        [Test]
        public void Should_advertise_response_trailers_capability()
            => transport.Supports(TransportCapabilities.ResponseTrailingHeaders).Should().BeTrue();
    }
}
