using System.IO;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal class ResponseFactory
    {
        private readonly WebRequestTransportSettings settings;

        public ResponseFactory(WebRequestTransportSettings settings)
            => this.settings = settings;

        public Response BuildSuccessResponse(WebRequestState state)
            => BuildResponse((ResponseCode)(int)state.Response.StatusCode, state);

        public Response BuildFailureResponse(HttpActionStatus status, WebRequestState state)
        {
            var headers = CreateResponseHeaders(state);
            switch (status)
            {
                case HttpActionStatus.ConnectionFailure:
                    return new Response(ResponseCode.ConnectFailure, headers: headers);

                case HttpActionStatus.SendFailure:
                    return new Response(ResponseCode.SendFailure);

                case HttpActionStatus.ReceiveFailure:
                    return new Response(ResponseCode.ReceiveFailure, headers: headers);

                case HttpActionStatus.Timeout:
                    return new Response(ResponseCode.RequestTimeout, headers: headers);

                case HttpActionStatus.RequestCanceled:
                    return new Response(ResponseCode.Canceled, headers: headers);

                case HttpActionStatus.InsufficientStorage:
                    return new Response(ResponseCode.InsufficientStorage, headers: headers);

                case HttpActionStatus.UserStreamFailure:
                    return new Response(ResponseCode.StreamInputFailure);

                default:
                    return BuildResponse(ResponseCode.UnknownFailure, state);
            }
        }

        public Response BuildResponse(ResponseCode code, WebRequestState state)
        {
            return new Response(
                code,
                CreateResponseContent(state),
                CreateResponseHeaders(state),
                CreateResponseStream(state)
            );
        }

        private Content CreateResponseContent(WebRequestState state)
        {
            if (state.ReturnStreamDirectly)
                return null;

            if (state.BodyBuffer != null)
                return new Content(state.BodyBuffer, 0, state.BodyBufferLength);

            if (state.BodyStream != null)
                return new Content(state.BodyStream.GetBuffer(), 0, (int)state.BodyStream.Position);

            return null;
        }

        private Headers CreateResponseHeaders(WebRequestState state)
        {
            var headers = Headers.Empty;

            if (state.Response == null)
                return headers;

            foreach (var key in state.Response.Headers.AllKeys)
            {
                var headerValue = state.Response.Headers[key];

                if (settings.FixNonAsciiHeaders)
                    headerValue = NonAsciiHeadersFixer.FixResponseHeaderValue(headerValue);

                headers = headers.Set(key, headerValue);
            }

            return headers;
        }

        private Stream CreateResponseStream(WebRequestState state) =>
            state.ReturnStreamDirectly ? new ResponseBodyStream(state) : null;
    }
}