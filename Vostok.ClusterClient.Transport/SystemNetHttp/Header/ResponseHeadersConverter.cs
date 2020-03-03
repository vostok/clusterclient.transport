using System.Collections.Generic;
using System.Net.Http;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Header
{
    internal static class ResponseHeadersConverter
    {
        public static Headers Convert(HttpResponseMessage responseMessage)
        {
            var headers = Headers.Empty;

            if (responseMessage?.Headers != null)
            {
                foreach (var pair in responseMessage.Headers)
                    headers = headers.Set(pair.Key, FlattenValue(pair.Value));
            }

            if (responseMessage?.Content?.Headers != null)
            {
                foreach (var pair in responseMessage.Content.Headers)
                    headers = headers.Set(pair.Key, FlattenValue(pair.Value));
            }

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
