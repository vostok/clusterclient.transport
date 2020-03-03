using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Webrequest
{
    [TestFixture]
    internal class WebRequestTransport_Tests : RuntimeSpecificFixture
    {
        private WebRequestTransport transport;

        [SetUp]
        public void TestSetup()
            => transport = new WebRequestTransport(new SynchronousConsoleLog());

        [Test]
        public void Should_advertise_request_composite_body_capability()
            => transport.Supports(TransportCapabilities.RequestCompositeBody).Should().BeTrue();

        [Test]
        public void Should_advertise_request_streaming_capability()
            => transport.Supports(TransportCapabilities.RequestStreaming).Should().BeTrue();

        [Test]
        public void Should_advertise_response_streaming_capability()
            => transport.Supports(TransportCapabilities.ResponseStreaming).Should().BeTrue();

        protected override Runtime SupportedRuntimes => Runtime.Framework | Runtime.Mono;
    }
}
