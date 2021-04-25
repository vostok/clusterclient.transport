using Vostok.Clusterclient.Core.Transport;
using Vostok.Commons.Environment;
using Vostok.Logging.Abstractions;

#pragma warning disable 618

namespace Vostok.Clusterclient.Transport.Universal
{
    internal static class TransportFactory
    {
        public static ITransport Create(UniversalTransportSettings settings, ILog log)
        {
            return new SocketsTransport(settings.ToSocketsTransportSettings(), log);
        }
    }
}
