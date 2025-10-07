#if NET5_0_OR_GREATER
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Time;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Sockets;

public class IpV6Tests
{
    [Test]
    public async Task Should_work_with_ipv6_address()
    {
        using var server = await KestrelTestServer.StartNewAsync(ctx =>
        {
            ctx.Response.StatusCode = 200;
        });
        var transport = new SocketsTransport(new SocketsTransportSettings
            {
                TcpKeepAliveEnabled = true,
                TcpKeepAliveInterval = 5.Seconds(),
                TcpKeepAliveTime = 5.Seconds(),
            },
            new ConsoleLog());

        var address = Dns.GetHostAddresses("localhost").First(x => x.AddressFamily == AddressFamily.InterNetworkV6);
        var url = new Uri($"http://[{address}]:{server.Port}");
        var response = transport.SendAsync(Request.Post(url), 750.Milliseconds(), 3.Seconds(), CancellationToken.None);

        response.Result.Code.Should().Be(200);
    }
}
#endif