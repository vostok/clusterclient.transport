#if NET5_0_OR_GREATER
using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Time;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Sockets;

internal class TcpKeepAliveOptionsTest
{
    /// <summary>
    /// Starting with .net5, a specialized keepalive management API appeared for a SocketsHttpHandler.
    /// And starting with .net7, the way we did before the appearance of .net5 broke
    /// </summary>
    [Test]
    public void Should_send_large_request_body_with_keepalive_option()
    {
        const int requestSize = 5 * 1024 * 1024;
        var responseSize = 0;
        
        using (var server = KestrelTestServer.StartNew(ctx =>
               {
                   var buffer = new byte[64 * 1024];
                   while (true)
                   {
                       var bytesReceived = ctx.Request.Body.ReadAsync(buffer, 0, buffer.Length).GetAwaiter().GetResult();
                       if (bytesReceived == 0)
                           break;

                       Interlocked.Add(ref responseSize, bytesReceived);
                   }

                   ctx.Response.StatusCode = 200;
               }))
        {
            var transport = new SocketsTransport(new SocketsTransportSettings
                {
                    TcpKeepAliveEnabled = true,
                    TcpKeepAliveInterval = 5.Seconds(),
                    TcpKeepAliveTime = 5.Seconds(),
                },
                new ConsoleLog());
            var response = transport.SendAsync(Request.Post(server.Url).WithContent(new byte[requestSize]), 750.Milliseconds(), 3.Seconds(), CancellationToken.None);

            response.Result.Code.Should().Be(200);
            responseSize.Should().Be(requestSize);
        }
    }
}

#endif