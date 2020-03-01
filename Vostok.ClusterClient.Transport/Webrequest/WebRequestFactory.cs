using System;
using System.Net;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal static class WebRequestFactory
    {
        public static HttpWebRequest Create(Request request, TimeSpan timeout, WebRequestTransportSettings settings, ILog log)
        {
            var webRequest = WebRequest.CreateHttp(request.Url);

            webRequest.Method = request.Method;

            WebRequestTuner.Tune(webRequest, timeout, settings);

            if (settings.FixNonAsciiHeaders)
                request = NonAsciiHeadersFixer.Fix(request);

            WebRequestHeadersFiller.Fill(request, webRequest, log);

            return webRequest;
        }
    }
}
