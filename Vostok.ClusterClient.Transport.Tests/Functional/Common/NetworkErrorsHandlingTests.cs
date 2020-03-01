using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class NetworkErrorsHandlingTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_be_able_to_transform_response_to_clear_response_without_partial_received_headers_and_body_if_network_error_happens()
        {
            const string customHeader = "MyCustomHeader";
            var content = ThreadSafeRandom.NextBytes(1024);

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = content.Length;
                    ctx.Response.Headers.Add(customHeader, "HeaderValue");
                    ctx.Response.OutputStream.Write(content, 0, content.Length / 2);
                    ctx.Response.Abort();
                }))
            {
                var response = Send(Request.Get(server.Url));

                response.Code.Should().Be(ResponseCode.ReceiveFailure);
                response.Headers.Any(h => h.Name == customHeader).Should().Be(false);
                response.HasContent.Should().BeFalse();
                response.HasStream.Should().BeFalse();
            }
        }
    }
}
