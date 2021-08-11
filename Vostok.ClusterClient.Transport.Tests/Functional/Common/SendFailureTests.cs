using System.Net.Sockets;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Environment;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class SendFailureTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_return_SendFailure_code_when_socket_closed_while_sending_large_body()
        {
            if (!RuntimeDetector.IsDotNetCore21AndNewer)
            {
                return;
            }

            var contentLargerThanTcpPacket = ThreadSafeRandom.NextBytes(64 * 1024 + 10);
            using (var stream = new BlockingStream(contentLargerThanTcpPacket))
            {
                using (var server = SocketTestServer.StartNew("",
                    // ReSharper disable once AccessToDisposedClosure
                    onBeforeRequestReading: c => CloseConnectionAndUnblockStream(c, stream)))
                {
                    var request = Request
                        .Put(server.Url)
                        .WithContent(stream);
                    var response = Send(request);

                    response.Code.Should().Be(ResponseCode.SendFailure);
                }
            }
        }

        private static void CloseConnectionAndUnblockStream(TcpClient client, BlockingStream blockingStream)
        {
            try
            {
                client.Close();
            }
            finally
            {
                blockingStream.UnblockForOneRead();
            }
        }
    }
}