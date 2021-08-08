using System;
using System.Net.Sockets;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class SendFailureTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_return_SendFailure_code_when_socket_closed_while_sending_large_body()
        {
            using(var server = SocketTestServer.StartNew("",
                onBeforeRequestReading: CloseUnderliningConnection))
            {
                var contentLargerThanTcpPacket = ThreadSafeRandom.NextBytes(64 * 1024 + 10);
                var stream = new SlowStream(contentLargerThanTcpPacket);
                var request = Request
                    .Put(server.Url)
                    .WithContent(stream);
                var response = Send(request);

                response.Code.Should().Be(ResponseCode.SendFailure);
            }
        }

        private static void CloseUnderliningConnection(TcpClient client) => client.Close();
    }
}