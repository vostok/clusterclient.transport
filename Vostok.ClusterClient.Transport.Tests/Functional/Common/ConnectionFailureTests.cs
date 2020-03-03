using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers.Network;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class ConnectionFailureTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_return_ConnectFailure_code_when_request_url_is_wrong()
        {
            var response = Send(Request.Get("http://255.255.255.255/"));

            response.Code.Should().Be(ResponseCode.ConnectFailure);
        }

        [Test]
        public void Should_return_ConnectFailure_code_when_request_url_contains_unresolvable_hostname()
        {
            var response = Send(Request.Get("http://sdoghisguhodfgkdjfgdsfgj:7545/"));

            response.Code.Should().Be(ResponseCode.ConnectFailure);
        }

        [Test]
        public void Should_return_ConnectFailure_code_when_server_does_not_listen_on_needed_port()
        {
            var response = Send(Request.Get($"http://localhost:{FreeTcpPortFinder.GetFreePort()}/"));

            response.Code.Should().Be(ResponseCode.ConnectFailure);
        }
    }
}
