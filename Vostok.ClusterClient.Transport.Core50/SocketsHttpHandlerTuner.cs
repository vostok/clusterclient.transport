﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
                            var resolvedIps = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);

                            var addressesToSend = FilterIPs(resolvedIps);
                            if (addressesToSend.Count == 0)
                            {
                                throw new Exception($"{host} DNS lookup failed");
                            }

                            await socket.ConnectAsync(addressesToSend[ThreadSafeRandom.Next(addressesToSend.Count)], context.DnsEndPoint.Port, token).ConfigureAwait(false);
                        }
                        else
                        {
                            await socket.ConnectAsync(context.DnsEndPoint, token).ConfigureAwait(false);
                        }

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

        private static ArraySegment<IPAddress> FilterIPs(IPAddress[] resolvedIps)
        {
            if (resolvedIps.Length == 1)
            {
                return NotValidIp(resolvedIps[0])
                    ? ArraySegment<IPAddress>.Empty
                    : new ArraySegment<IPAddress>(resolvedIps);
            }

            var addresses = new IPAddress[resolvedIps.Length];
            var count = 0;
            foreach (var resolvedIp in resolvedIps)
            {
                if (NotValidIp(resolvedIp))
                    continue;

                addresses[count] = resolvedIp;
                count++;
            }

            return new ArraySegment<IPAddress>(addresses, 0, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool NotValidIp(IPAddress ip) =>
            ip.AddressFamily is not (AddressFamily.InterNetwork);
    }
}