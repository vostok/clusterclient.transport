using Vostok.Clusterclient.Core.Transport;
using Vostok.ClusterClient.Transport.Sockets;
using Vostok.Logging.Abstractions;

// ReSharper disable once CheckNamespace
namespace Vostok.Clusterclient.Transport.Adapter
{
    public static class TransportFactory
    {
        public static ITransport Create(object rawSettings, ILog log)
        {
            var settings = Translator.Translate<UniversalTransportSettings>(rawSettings);
            var webRequestTransportSettings = new SocketsTransportSettings
            {
                AllowAutoRedirect = settings.AllowAutoRedirect,
                RequestAbortTimeout = settings.RequestAbortTimeout,
                UseResponseStreaming = settings.UseResponseStreaming,
                MaxResponseBodySize = settings.MaxResponseBodySize,
                TcpKeepAliveEnabled = settings.TcpKeepAliveEnabled,
                TcpKeepAliveInterval = settings.TcpKeepAliveInterval,
                TcpKeepAliveTime = settings.TcpKeepAliveTime
            };
            
            return new SocketsTransport(webRequestTransportSettings, log);
        }
    }
}