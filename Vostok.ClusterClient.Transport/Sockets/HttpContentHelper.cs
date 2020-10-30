using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Sockets
{
    internal static class HttpContentHelper
    {
        private static readonly object Sync = new object();
        private static readonly Action<HttpContent, HttpContentHeaders> EmptyHeadersSetter = delegate {};

        private static volatile Action<HttpContent, HttpContentHeaders> headersSetter;
        private static volatile bool canSetHeaders;

        public static void SetHttpContentHeaders(HttpContent targetContent, HttpContentHeaders headers, ILog log)
        {
            EnsureInitialized(log);

            if (!canSetHeaders)
            {
                CopyHeaders(targetContent, headers);
                return;
            }

            try
            {
                headersSetter(targetContent, headers);
            }
            catch (Exception e)
            {
                log.ForContext(typeof(HttpContentHelper)).Warn(e, "Failed to set HttpContentHeaders.");
                canSetHeaders = false;
                CopyHeaders(targetContent, headers);
            }
        }

        private static void EnsureInitialized(ILog log)
        {
            if (headersSetter != null)
                return;

            lock (Sync)
            {
                if (headersSetter == null)
                {
                    var setter = BuildSetter(log);

                    if (Test(setter, log))
                    {
                        headersSetter = setter;
                        canSetHeaders = true;
                        return;
                    }

                    headersSetter = EmptyHeadersSetter;
                }
            }
        }

        private static Action<HttpContent, HttpContentHeaders> BuildSetter(ILog log)
        {
            try
            {
                var headersField = typeof(HttpContent).GetField("_headers", BindingFlags.Instance | BindingFlags.NonPublic);
                var contentParameter = Expression.Parameter(typeof(HttpContent), "to");
                var contentHeaderParameter = Expression.Parameter(typeof(HttpContentHeaders), "source");
                var headersFieldExp = Expression.Field(contentParameter, headersField);
                var assignment = Expression.Assign(headersFieldExp, contentHeaderParameter);

                return Expression.Lambda<Action<HttpContent, HttpContentHeaders>>(assignment, contentParameter, contentHeaderParameter).Compile();
            }
            catch (Exception e)
            {
                log.ForContext(typeof(HttpContentHelper)).Warn(e);
                return EmptyHeadersSetter;
            }
        }

        private static bool Test(Action<HttpContent, HttpContentHeaders> setter, ILog log)
        {
            (string name, string value)[] testsHeaders =
            {
                (HeaderNames.ContentType, "json"),
                (HeaderNames.ContentEncoding, "identity")
            };

            try
            {
                using (var targetContent = new BufferContent(new Content(new byte[100]), CancellationToken.None))
                {
                    using (var sourceContent = new BufferContent(new Content(new byte[100]), CancellationToken.None))
                    {
                        foreach (var header in testsHeaders)
                            sourceContent.Headers.Add(header.name, header.value);

                        setter(targetContent, sourceContent.Headers);

                        var headers = targetContent.Headers;

                        foreach (var (name, expectedValue) in testsHeaders)
                        {
                            if (!headers.TryGetValues(name, out var values) || !values.Contains(expectedValue))
                            {
                                log.ForContext(typeof(HttpContentHelper)).Warn("Can't set headers to new content");

                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                log.ForContext(typeof(HttpContentHelper)).Warn(error, "Failed to build HttpContentHeaders setter");
                return false;
            }

            return true;
        }

        private static void CopyHeaders(HttpContent targetContent, HttpContentHeaders headers)
        {
            if (headers == null)
                return;

            foreach (var header in headers)
                targetContent.Headers.Add(header.Key, header.Value);
        }
    }
}