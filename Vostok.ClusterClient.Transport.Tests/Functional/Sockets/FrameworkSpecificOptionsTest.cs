using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Environment;
using Vostok.Commons.Time;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Sockets;

internal class FrameworkSpecificOptionsTest
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

        using (var server = TestServer.StartNew(ctx => { ctx.Response.StatusCode = 200; }))
        {
            server.BufferRequestBody = false;

            var transport = new SocketsTransport(new SocketsTransportSettings
            {
                TcpKeepAliveEnabled = true, 
                TcpKeepAliveInterval = 5.Seconds(),
                TcpKeepAliveTime = 5.Seconds(),
            }, new ConsoleLog());
            var response = transport.SendAsync(Request.Post(server.Url).WithContent(new byte[size]), 750.Milliseconds(), 10.Minutes(), CancellationToken.None);

            response.Result.Code.Should().Be(200);
            server.LastRequest.BodySize.Should().Be(size);
        }
    }
    
#if NET6_0_OR_GREATER
    [Test]
    public void Should_modify_headers_if_specified_option_is_set()
    {
        using (var server = TestServer.StartNew(ctx => { ctx.Response.StatusCode = 200; }))
        {
            var transport = new SocketsTransport(new SocketsTransportSettings
                {
                    HeadersModifier = (httpRequestHeaders, httpContentHeaders) =>
                    {
                        if (httpContentHeaders != null
                            && httpContentHeaders.TryGetValues("Content-Length", out var values)
                            && values.FirstOrDefault() == "0")
                            httpContentHeaders.Remove("Content-Length");
                    }
                },
                new ConsoleLog());
                
            var response = transport.SendAsync(Request.Post(server.Url), 250.Milliseconds(), 10.Minutes(), CancellationToken.None);

            response.Result.Code.Should().Be(200);
            server.LastRequest.Headers["Content-Length"].Should().BeNull();
        }
    }
#endif
}