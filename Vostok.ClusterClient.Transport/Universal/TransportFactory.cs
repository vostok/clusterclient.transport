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
            if (RuntimeDetector.IsDotNetCore21AndNewer)
                return new SocketsTransport(settings.ToSocketsTransportSettings(), log);
            
            if (RuntimeDetector.IsDotNetCore20)
                return new NativeTransport(settings.ToNativeTransportSettings(), log);

            if (RuntimeDetector.IsDotNetFramework)
                return new WebRequestTransport(settings.ToWebRequestTransportSettings(), log);

            log.Warn("Unknown .NET runtime. Will fall back to HttpWebRequest-based transport.");

            return new WebRequestTransport(settings.ToWebRequestTransportSettings(), log);
        }
    }
}
