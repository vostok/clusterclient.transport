using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class QuerySendingTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_correctly_transfer_query_parameters_to_server()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request
                    .Get(server.Url)
                    .WithAdditionalQueryParameter("key1", "value1")
                    .WithAdditionalQueryParameter("key2", "value2")
                    .WithAdditionalQueryParameter("ключ3", "значение3");

                Send(request);

                server.LastRequest.Query["key1"].Should().Be("value1");
                server.LastRequest.Query["key2"].Should().Be("value2");
                server.LastRequest.Query["ключ3"].Should().Be("значение3");
            }
        }
    }
}
