using System;
using System.Net;
using System.Net.Http.Headers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal static class WebRequestHeadersFiller
    {
        public static void Fill(Request request, HttpWebRequest webRequest, ILog log)
        {
            if (request.Headers != null)
            {
                var canAssignDirectly = WebRequestHeadersHacker.TryUnlockRestrictedHeaders(webRequest, log);
                if (canAssignDirectly)
                {
                    AssignHeadersDirectly(request.Headers, webRequest);
                }
                else
                {
                    AssignHeadersThroughProperties(request.Headers, webRequest);
                }
            }

            SetContentLengthHeader(request, webRequest);

            TrySetHostExplicitly(request, webRequest);
        }

        private static void AssignHeadersDirectly(Headers headers, HttpWebRequest webRequest)
        {
            foreach (var header in headers)
            {
                if (NeedToSkipHeader(header.Name))
                    continue;

                webRequest.Headers.Set(header.Name, header.Value);
            }
        }

        private static void AssignHeadersThroughProperties(Headers headers, HttpWebRequest webRequest)
        {
            foreach (var header in headers)
            {
                if (NeedToSkipHeader(header.Name))
                    continue;

                if (TryHandleSpecialHeaderWithProperty(webRequest, header))
                    continue;

                webRequest.Headers.Set(header.Name, header.Value);
            }
        }

        private static void SetContentLengthHeader(Request request, HttpWebRequest webRequest)
        {
            webRequest.ContentLength =
                request.Content?.Length ??
                request.CompositeContent?.Length ??
                request.StreamContent?.Length ??
                0;

            var streamContent = request.StreamContent;
            if (streamContent != null && streamContent.Length == null)
            {
                webRequest.SendChunked = true;
            }
        }

        private static void TrySetHostExplicitly(Request request, HttpWebRequest webRequest)
        {
            var host = request.Headers?[HeaderNames.Host];
            if (host != null)
                webRequest.Host = host;
        }

        private static bool TryHandleSpecialHeaderWithProperty(HttpWebRequest webRequest, Header header)
        {
            if (header.Name.Equals(HeaderNames.Accept))
            {
                webRequest.Accept = header.Value;
                return true;
            }

            if (header.Name.Equals(HeaderNames.ContentType))
            {
                webRequest.ContentType = header.Value;
                return true;
            }

            if (header.Name.Equals(HeaderNames.IfModifiedSince))
            {
                webRequest.IfModifiedSince = DateTime.Parse(header.Value);
                return true;
            }

            if (header.Name.Equals(HeaderNames.Range))
            {
                var ranges = RangeHeaderValue.Parse(header.Value);

                foreach (var range in ranges.Ranges)
                {
                    webRequest.AddRange(ranges.Unit, range.From ?? 0, range.To ?? 0);
                }

                return true;
            }

            if (header.Name.Equals(HeaderNames.Referer))
            {
                webRequest.Referer = header.Value;
                return true;
            }

            if (header.Name.Equals(HeaderNames.UserAgent))
            {
                webRequest.UserAgent = header.Value;
                return true;
            }

            if (header.Name.Equals(HeaderNames.Date))
            {
                webRequest.Date = DateTime.Parse(header.Value);
                return true;
            }

            return false;
        }

        private static bool NeedToSkipHeader(string name)
        {
            return
                name.Equals(HeaderNames.ContentLength) ||
                name.Equals(HeaderNames.Connection) ||
                name.Equals(HeaderNames.Host) ||
                name.Equals(HeaderNames.TransferEncoding);
        }
    }
}
