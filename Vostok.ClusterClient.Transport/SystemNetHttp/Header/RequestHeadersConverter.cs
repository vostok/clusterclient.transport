using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Header
{
    internal static class RequestHeadersConverter
    {
        public static void Fill(Request request, HttpRequestMessage message, ILog log)
        {
            if (request.Headers != null)
            {
                var canAssignDirectly = RequestHeadersUnlocker.TryUnlockRestrictedHeaders(message.Headers, log);
                if (canAssignDirectly)
                {
                    AssignHeadersDirectly(request.Headers, message.Headers);
                }
                else
                {
                    AssignHeadersThroughProperties(request.Headers, message);
                }
            }

            TrySetHostExplicitly(request.Headers, message.Headers);
        }

        private static void AssignHeadersDirectly(Headers source, HttpHeaders target)
        {
            foreach (var header in source)
            {
                if (NeedToSkipHeader(header.Name))
                    continue;

                if (!target.TryAddWithoutValidation(header.Name, header.Value))
                    throw new InvalidOperationException($"Failed to add header with name '{header.Name}' and value '{header.Value}'.");
            }
        }

        private static void AssignHeadersThroughProperties(Headers headers, HttpRequestMessage message)
        {
            foreach (var header in headers)
            {
                if (NeedToSkipHeader(header.Name))
                    continue;

                if (IsContentHeader(header.Name))
                {
                    message.Content.Headers.Add(header.Name, header.Value);
                    continue;
                }

                message.Headers.Add(header.Name, header.Value);
            }
        }

        private static bool NeedToSkipHeader(string name) => name.Equals(HeaderNames.Connection) ||
                                                             name.Equals(HeaderNames.ContentLength) ||
                                                             name.Equals(HeaderNames.Host) ||
                                                             name.Equals(HeaderNames.TransferEncoding);

        private static bool IsContentHeader(string headerName)
        {
            switch (headerName)
            {
                case HeaderNames.Allow:
                case HeaderNames.ContentDisposition:
                case HeaderNames.ContentEncoding:
                case HeaderNames.ContentLanguage:
                case HeaderNames.ContentLength:
                case HeaderNames.ContentLocation:
                case HeaderNames.ContentMD5:
                case HeaderNames.ContentRange:
                case HeaderNames.ContentType:
                case HeaderNames.Expires:
                case HeaderNames.LastModified:
                    return true;

                default:
                    return false;
            }
        }

        private static void TrySetHostExplicitly(Headers source, HttpRequestHeaders target)
        {
            var host = source?[HeaderNames.Host];
            if (host != null)
                target.Host = host;
        }
    }
}
