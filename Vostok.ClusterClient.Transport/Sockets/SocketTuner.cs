using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Vostok.Commons.Helpers.Network;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Sockets
{
    internal class SocketTuner
    {
        private readonly ILog log;
        private readonly bool arpWarmupEnabled;
        private readonly bool keepAliveEnabled;
        private readonly byte[] keepAliveValues;

        public SocketTuner(SocketsTransportSettings settings, ILog log)
        {
            this.log = log;

            arpWarmupEnabled = settings.ArpCacheWarmupEnabled && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            keepAliveEnabled = settings.TcpKeepAliveEnabled && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            keepAliveValues = keepAliveEnabled ? GetKeepAliveValues(settings) : null;
        }

        public bool CanTune => arpWarmupEnabled || keepAliveEnabled;

        public void Tune(Socket socket)
        {
            if (socket == null)
                return;

            try
            {
                if (arpWarmupEnabled && socket.RemoteEndPoint is IPEndPoint ipEndPoint)
                    ArpCacheMaintainer.ReportAddress(ipEndPoint.Address);
            }
            catch (Exception error)
            {
                log.Warn(error, "Failed to enable ARP cache warmup for replica address.");
            }

            try
            {
                if (keepAliveEnabled)
                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);
            }
            catch (Exception error)
            {
                log.Warn(error, "Failed to enable TCP keep-alive for socket.");
            }
        }

        private static byte[] GetKeepAliveValues(SocketsTransportSettings settings)
        {
            var tcpKeepAliveTime = (int)settings.TcpKeepAliveTime.TotalMilliseconds;
            var tcpKeepAliveInterval = (int)settings.TcpKeepAliveInterval.TotalMilliseconds;

            return new byte[]
            {
                1,
                0,
                0,
                0,
                (byte)(tcpKeepAliveTime & byte.MaxValue),
                (byte)((tcpKeepAliveTime >> 8) & byte.MaxValue),
                (byte)((tcpKeepAliveTime >> 16) & byte.MaxValue),
                (byte)((tcpKeepAliveTime >> 24) & byte.MaxValue),
                (byte)(tcpKeepAliveInterval & byte.MaxValue),
                (byte)((tcpKeepAliveInterval >> 8) & byte.MaxValue),
                (byte)((tcpKeepAliveInterval >> 16) & byte.MaxValue),
                (byte)((tcpKeepAliveInterval >> 24) & byte.MaxValue)
            };
        }
    }
}
