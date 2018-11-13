using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Native;
using Vostok.Logging.Abstractions;

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
                RequestAbortTimeout = settings.RequestAbortTimeout,
                UseResponseStreaming = settings.UseResponseStreaming,
                MaxResponseBodySize = settings.MaxResponseBodySize
            };
            
            return new NativeTransport(transportSettings, log);
        }
    }
}