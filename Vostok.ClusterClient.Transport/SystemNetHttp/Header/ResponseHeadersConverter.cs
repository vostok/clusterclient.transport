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
                headers = headers.Add(responseMessage.Headers);

            if (responseMessage?.Content?.Headers != null)
                headers = headers.Add(responseMessage.Content.Headers);

            return headers;
        }

        public static Headers Convert(HttpResponseHeaders responseHeaders)
        {
            return responseHeaders == null ? Headers.Empty : Headers.Empty.Add(responseHeaders);;
        }

        private static Headers Add(this Headers headers, HttpHeaders responseHeaders)
        {
            return responseHeaders.Aggregate(headers, (current, pair) => current.Set(pair.Key, FlattenValue(pair.Value)));
        }

        private static string FlattenValue(IEnumerable<string> value)
        {
            if (value is IList<string> valuesList && valuesList.Count == 1)
                return valuesList[0];

            return string.Join(",", value);
        }
    }
}
