using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Transport.Core50
{
    [UsedImplicitly]
    public static class SocketsHttpHandlerTuner
    {
        private static readonly Encoding HeadersEncoding = new UTF8Encoding(false);

        [UsedImplicitly]
        public static void Tune(HttpMessageHandler handler, bool tcpKeepAliveEnables, TimeSpan tcpKeepAliveInterval, TimeSpan tcpKeepAliveTime)
        {
            if (!(handler is SocketsHttpHandler socketHandler))
                return;

            if (tcpKeepAliveEnables)
            {
                var keepAliveTime = (int)tcpKeepAliveTime.TotalSeconds;
                var keepAliveInterval = (int)tcpKeepAliveInterval.TotalSeconds;
                
                socketHandler.ConnectCallback = async (context, token) =>
                {
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};
                    try
                    {
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, keepAliveTime);
                        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, keepAliveInterval);
                        // (deniaa, 08.06.2023): We can configure an option (SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, X) here, but we have no this option in transport settings model.

                        await socket.ConnectAsync(context.DnsEndPoint, token).ConfigureAwait(false);

                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                };
            }

            socketHandler.RequestHeaderEncodingSelector = (_, __) => HeadersEncoding;
            socketHandler.ResponseHeaderEncodingSelector = (_, __) => HeadersEncoding;

            socketHandler.ResponseDrainTimeout = TimeSpan.FromSeconds(3);
            socketHandler.MaxResponseDrainSize = 64 * 1024;
        }
    }
}
