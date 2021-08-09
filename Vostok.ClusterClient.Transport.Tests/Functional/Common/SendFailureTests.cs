using System.Net.Sockets;
using System.Threading;
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
            
            using (var autoResetEvent = new AutoResetEvent(false))
            {
                using (var server = SocketTestServer.StartNew("",
                    // ReSharper disable once AccessToDisposedClosure
                    onBeforeRequestReading: c => CloseUnderliningConnectionAndSetResult(c, autoResetEvent)))
                {
                    var contentLargerThanTcpPacket = ThreadSafeRandom.NextBytes(64 * 1024 + 10);
                    var stream = new BlockingStream(contentLargerThanTcpPacket, autoResetEvent);
                    var request = Request
                        .Put(server.Url)
                        .WithContent(stream);
                    var response = Send(request);

                    response.Code.Should().Be(ResponseCode.SendFailure);
                }
            }
        }

        private static void CloseUnderliningConnectionAndSetResult(TcpClient client, AutoResetEvent autoResetEvent)
        {
            try
            {
                client.Close();
            }
            finally
            {
                autoResetEvent.Set();
            }
        }
    }
}