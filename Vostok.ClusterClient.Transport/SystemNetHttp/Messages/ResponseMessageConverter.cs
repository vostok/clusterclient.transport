using System.IO;
using System.Net.Http;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Header;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Messages
{
    internal static class ResponseMessageConverter
    {
        public static Response Convert(
            [NotNull] HttpResponseMessage message,
            [CanBeNull] Content content,
            [CanBeNull] Stream stream)
        {
            var code = (ResponseCode)(int)message.StatusCode;

            var headers = ResponseHeadersConverter.Convert(message);

            return new Response(code, content, headers, stream);
        }
    }
}
