using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Header
{
    internal static class ResponseHeadersConverter
    {
        public static Headers Convert(HttpResponseMessage responseMessage)
        {
            var headers = Headers.Empty;

            if (responseMessage?.Headers != null)
                headers = Add(headers, responseMessage.Headers);

            if (responseMessage?.Content?.Headers != null)
                headers = Add(headers, responseMessage.Content.Headers);

            return headers;
        }

        public static Headers Convert(HttpResponseHeaders responseHeaders)
        {
            return responseHeaders == null ? Headers.Empty : Add(Headers.Empty, responseHeaders);;
        }

        private static Headers Add(Headers headers, HttpHeaders responseHeaders)
        {
            foreach (var pair in responseHeaders)
                headers = headers.Set(pair.Key, FlattenValue(pair.Value));
            return headers;
        }

        private static string FlattenValue(IEnumerable<string> value)
        {
            if (value is IList<string> valuesList && valuesList.Count == 1)
                return valuesList[0];

            return string.Join(",", value);
        }
    }
}
