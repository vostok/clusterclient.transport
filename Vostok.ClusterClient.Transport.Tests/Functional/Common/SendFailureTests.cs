using System;
using System.Net.Sockets;
using System.Threading.Tasks;
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

            var source = new TaskCompletionSource<object>();
            using (var server = SocketTestServer.StartNew("",
                onBeforeRequestReading: c => CloseUnderliningConnectionAndSetResult(c, source)))
            {
                var contentLargerThanTcpPacket = ThreadSafeRandom.NextBytes(64 * 1024 + 10);
                var stream = new BlockingOnNonZeroOffsetStream(contentLargerThanTcpPacket, source);
                var request = Request
                    .Put(server.Url)
                    .WithContent(stream);
                var response = Send(request);

                response.Code.Should().Be(ResponseCode.SendFailure);
            }
        }

        private static void CloseUnderliningConnectionAndSetResult(TcpClient client, TaskCompletionSource<object> taskCompletionSource)
        {
            try
            {
                client.Close();
            }
            finally
            {
                taskCompletionSource.SetResult(null);
            }
        }
    }
}