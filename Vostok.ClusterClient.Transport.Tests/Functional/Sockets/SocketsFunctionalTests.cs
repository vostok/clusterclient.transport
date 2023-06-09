using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Environment;
using Vostok.Commons.Time;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Sockets
{
    internal class TcpKeepAliveOptionsTest
    {
        [SetUp]
        public void CheckRuntime()
        {
            if (!RuntimeDetector.IsDotNetCore21AndNewer)
                Assert.Pass();
        }

        /// <summary>
        /// Starting with .net5, a specialized keepalive management API appeared for a SocketsHttpHandler.
        /// And starting with .net7, the way we did before the appearance of .net5 broke
        /// </summary>
        [Test]
        public void Should_send_large_request_body_with_keepalive_option()
        {
            const long size = 5L * 1024 * 1024;

            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                server.BufferRequestBody = true;

                var transport = new SocketsTransport(new SocketsTransportSettings
                {
                    TcpKeepAliveEnabled = false
                }, new ConsoleLog());
                var response = transport.SendAsync(Request.Post(server.Url).WithContent(new byte[size]), 1500.Milliseconds(), 10.Minutes(), CancellationToken.None);

                response.Result.Code.Should().Be(200);
                server.LastRequest.BodySize.Should().Be(size);
            }
        }
    }

    [Explicit]
    internal class AllowAutoRedirectTests : AllowAutoRedirectTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ClientTimeoutTests : ClientTimeoutTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ConnectionFailureTests : ConnectionFailureTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class SendFailureTests : SendFailureTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ConnectionTimeoutTests : ConnectionTimeoutTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ContentReceivingTests : ContentReceivingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ContentSendingTests : ContentSendingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ContentStreamingTests : ContentStreamingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class HeaderReceivingTests : HeaderReceivingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class HeaderSendingTests : HeaderSendingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class MaxConnectionsPerEndpointTests : MaxConnectionsPerEndpointTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class MethodSendingTests : MethodSendingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class NetworkErrorsHandlingTests : NetworkErrorsHandlingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class ProxyTests : ProxyTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class QuerySendingTests : QuerySendingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class RequestCancellationTests : RequestCancellationTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class StatusCodeReceivingTests : StatusCodeReceivingTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class RemoteCertificateValidationTests : RemoteCertificateValidationTests<SocketsTestsConfig>
    {
    }

    [Explicit]
    internal class CredentialsTests : CredentialsTests<SocketsTestsConfig>
    {
    }
}