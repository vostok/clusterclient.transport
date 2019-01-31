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
                ConnectionIdleTimeout = settings.ConnectionIdleTimeout,
                RequestAbortTimeout = settings.RequestAbortTimeout,
                UseResponseStreaming = settings.UseResponseStreaming,
                MaxResponseBodySize = settings.MaxResponseBodySize,
                MaxConnectionsPerEndpoint = settings.MaxConnectionsPerEndpoint,
                TcpKeepAliveEnabled = settings.TcpKeepAliveEnabled,
                TcpKeepAliveInterval = settings.TcpKeepAliveInterval,
                TcpKeepAliveTime = settings.TcpKeepAliveTime,
                Proxy = settings.Proxy
            };

            return new SocketsTransport(transportSettings, log);
        }
    }
}