using System.Net.Http;
using System.Threading;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Clusterclient.Transport.SystemNetHttp.Header;
using Vostok.Logging.Abstractions;
using StreamContent = Vostok.Clusterclient.Transport.SystemNetHttp.Contents.StreamContent;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Messages
{
    internal static class RequestMessageFactory
    {
        private static readonly HttpMethod PatchMethod = new HttpMethod(RequestMethods.Patch);

        public static HttpRequestMessage Create(Request request, CancellationToken token, ILog log)
        {
            var message = new HttpRequestMessage(TranslateMethod(request.Method), request.Url)
            {
                Content = TranslateContent(request, token)
            };

            RequestHeadersConverter.Fill(request, message, log);

            return message;
        }

        private static HttpMethod TranslateMethod(string method)
        {
            return method switch
            {
                RequestMethods.Get => HttpMethod.Get,
                RequestMethods.Post => HttpMethod.Post,
                RequestMethods.Put => HttpMethod.Put,
                RequestMethods.Patch => PatchMethod,
                RequestMethods.Delete => HttpMethod.Delete,
                RequestMethods.Head => HttpMethod.Head,
                RequestMethods.Options => HttpMethod.Options,
                RequestMethods.Trace => HttpMethod.Trace,
                _ => new HttpMethod(method)
            };
        }

        private static HttpContent TranslateContent(Request request, CancellationToken cancellationToken)
        {
            if (request.Content != null && request.Content.Length > 0)
                return new BufferContent(request.Content, cancellationToken);

            if (request.CompositeContent != null && request.CompositeContent.Length > 0)
                return new CompositeBufferContent(request.CompositeContent, cancellationToken);

            if (request.StreamContent != null)
                return new StreamContent(request.StreamContent, cancellationToken);

            if (request.ContentProducer != null)
                return new ContentProducerContent(request.ContentProducer, cancellationToken);

            // (deniaa, 14.12.2023): A special EmptyContent with the logic for setting the Content-Length: 0 header is needed for the popular scenario with server throttling.
            // If the backend wants to respond with 429, a modern dotnet server will still read the entire body so as not to break the http 1.1 protocol. And the body can be big.
            // Therefore, if there is a body, instead of responding with 429, the throttling module will close the TCP connection.
            // And in order to clearly detect the absence of a body when it is not there, and not to break the connection, we always set Content-Length: 0.
            return new EmptyContent();
        }
    }
}
