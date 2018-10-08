using Vostok.ClusterClient.Core.Transport;
using Vostok.ClusterClient.Transport.Sockets;
using Vostok.Logging.Abstractions;

// ReSharper disable once CheckNamespace
namespace Vostok.ClusterClient.Transport.Adapter
{
    public static class TransportFactory
    {
        public static ITransport Create(object rawSettings, ILog log)
        {
            var settings = Translator.Translate<UniversalTransportSettings>(rawSettings);
            var webRequestTransportSettings = new SocketsTransportSettings
            {
                ConnectionAttempts = settings.ConnectionAttempts,
                ConnectionTimeout = settings.ConnectionTimeout,
                AllowAutoRedirect = settings.AllowAutoRedirect,
                RequestAbortTimeout = settings.RequestAbortTimeout,
                UseResponseStreaming = settings.UseResponseStreaming,
                MaxResponseBodySize = settings.MaxResponseBodySize
            };
            
            return new SocketsTransport(webRequestTransportSettings, log);
        }
    }
}