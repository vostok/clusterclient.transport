using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Logging.Abstractions;

// ReSharper disable once CheckNamespace
namespace Vostok.Clusterclient.Transport.Adapter
{
    public static class TransportFactory
    {
        public static ITransport Create(object rawSettings, ILog log)
        {
            var settings = Translator.Translate<UniversalTransportSettings>(rawSettings);
            var transportSettings = new SocketsTransportSettings
            {
                AllowAutoRedirect = settings.AllowAutoRedirect,
                ArpCacheWarmupEnabled = false,
                BufferFactory = settings.BufferFactory,
                ConnectionIdleTimeout = settings.ConnectionIdleTimeout,
                MaxConnectionsPerEndpoint = settings.MaxConnectionsPerEndpoint,
                MaxResponseBodySize = settings.MaxResponseBodySize,
                Proxy = settings.Proxy,
                RequestAbortTimeout = settings.RequestAbortTimeout,
                TcpKeepAliveEnabled = settings.TcpKeepAliveEnabled,
                TcpKeepAliveInterval = settings.TcpKeepAliveInterval,
                TcpKeepAliveTime = settings.TcpKeepAliveTime,
                UseResponseStreaming = settings.UseResponseStreaming
            };

            return new SocketsTransport(transportSettings, log);
        }
    }
}