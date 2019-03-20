using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Native;
using Vostok.Logging.Abstractions;
#pragma warning disable 618

// ReSharper disable once CheckNamespace
namespace Vostok.Clusterclient.Transport.Adapter
{
    public static class TransportFactory
    {
        public static ITransport Create(object rawSettings, ILog log)
        {
            var settings = Translator.Translate<UniversalTransportSettings>(rawSettings);

            var transportSettings = new NativeTransportSettings
            {
                AllowAutoRedirect = settings.AllowAutoRedirect,
                BufferFactory = settings.BufferFactory,
                MaxConnectionsPerEndpoint = settings.MaxConnectionsPerEndpoint,
                MaxResponseBodySize = settings.MaxResponseBodySize,
                Proxy = settings.Proxy,
                RequestAbortTimeout = settings.RequestAbortTimeout,
                UseResponseStreaming = settings.UseResponseStreaming
            };

            return new NativeTransport(transportSettings, log);
        }
    }
}