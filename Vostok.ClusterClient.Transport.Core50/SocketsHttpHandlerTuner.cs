using System;
using System.Net.Http;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Transport.Core50
{
    [UsedImplicitly]
    public static class SocketsHttpHandlerTuner
    {
        private static readonly Encoding HeadersEncoding = new UTF8Encoding(false);

        [UsedImplicitly]
        public static void Tune(HttpMessageHandler handler)
        {
            if (!(handler is SocketsHttpHandler socketHandler))
                return;

            socketHandler.RequestHeaderEncodingSelector = (_, __) => HeadersEncoding;
            socketHandler.ResponseHeaderEncodingSelector = (_, __) => HeadersEncoding;

            socketHandler.ResponseDrainTimeout = TimeSpan.FromSeconds(3);
            socketHandler.MaxResponseDrainSize = 64 * 1024;
        }
    }
}
