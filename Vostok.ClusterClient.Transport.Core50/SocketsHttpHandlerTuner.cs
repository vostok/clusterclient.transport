using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Transport.Core50
{
    [UsedImplicitly]
    public static class SocketsHttpHandlerTuner
    {
        private static readonly Encoding HeadersEncoding = new UTF8Encoding(false);
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

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

                        if (!IsWindows)
                        {
                            var host = context.DnsEndPoint.Host;
                            // https://github.com/dotnet/runtime/issues/24917
                            var ips = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                            if (ips.Length == 0)
                            {
                                throw new Exception($"{host} DNS lookup failed");
                            }

                            foreach (var ip in ips)
                            {
                                Console.WriteLine($"Host = {host}, AddressFamily = {ip.AddressFamily}, ip = {ip}, IsIPv6SiteLocal = {ip.IsIPv6SiteLocal}, IsIPv6LinkLocal = {ip.IsIPv6LinkLocal}");
                            }

                            ips = ips.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray();
                            
                            await socket.ConnectAsync(host, context.DnsEndPoint.Port, token).ConfigureAwait(false);
                        }
                        else
                        {
                            await socket.ConnectAsync(context.DnsEndPoint, token).ConfigureAwait(false);
                        }

                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch(Exception e)
                    {
                        Console.Out.WriteLine(e);
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