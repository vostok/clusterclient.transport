using System.Net.Http;

namespace Vostok.Clusterclient.Transport.Helpers;

internal static class NetCore60Utils
{
    public static void TuneHandler(HttpMessageHandler handler)
    {
#if NET6_0_OR_GREATER
        if (handler is not SocketsHttpHandler socketsHandler)
            return;

        // note (ponomaryovigor):
        // Disable Http Activity native creation because ClusterClient implements tracing manually.
        socketsHandler.ActivityHeadersPropagator = null;
#endif
    }
}