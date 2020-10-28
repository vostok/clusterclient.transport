using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Clusterclient.Transport.SystemNetHttp.BodyReading;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Clusterclient.Transport.SystemNetHttp.Header;
using Vostok.Clusterclient.Transport.SystemNetHttp.Helpers;
using Vostok.Clusterclient.Transport.SystemNetHttp.Messages;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport
{
    /// <summary>
    ///     <para>ClusterClient HTTP transport for .NET Core 2.1 and later.</para>
    ///     <para>Internally uses <c>SocketsHttpHandler</c>.</para>
    /// </summary>
    [PublicAPI]
    public class SocketsTransport : ITransport
    {
        private static readonly SocketsTransportSettings DefaultSettings = new SocketsTransportSettings();

        private readonly SocketsTransportSettings settings;
        private readonly ILog log;

        private readonly SocketsHandlerProvider handlerProvider;
        private readonly TimeoutProvider timeoutProvider;
        private readonly ErrorHandler errorHandler;
        private readonly SocketTuner socketTuner;
        private readonly BodyReader bodyReader;

        public SocketsTransport([NotNull] ILog log)
            : this(DefaultSettings, log)
        {
        }

        public SocketsTransport([NotNull] SocketsTransportSettings settings, [NotNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log ?? throw new ArgumentNullException(nameof(log));

            handlerProvider = new SocketsHandlerProvider(settings);
            timeoutProvider = new TimeoutProvider(settings.RequestAbortTimeout, this.log);
            errorHandler = new ErrorHandler(this.log);
            socketTuner = new SocketTuner(settings, this.log);
            bodyReader = new BodyReader(
                settings.BufferFactory,
                len => settings.UseResponseStreaming(len),
                () => settings.MaxResponseBodySize,
                this.log);
        }

        /// <inheritdoc />
        public TransportCapabilities Capabilities
            => TransportCapabilities.RequestCompositeBody |
               TransportCapabilities.RequestStreaming |
               TransportCapabilities.ResponseStreaming;

        /// <inheritdoc />
        public Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken token)
            => timeoutProvider.SendWithTimeoutAsync((r, t) => SendAsync(r, connectionTimeout, t), request, timeout, token);

        private async Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, CancellationToken token)
        {
            try
            {
                using (var state = new DisposableState())
                {
                    state.Request = RequestMessageFactory.Create(request, token, log);
                    if (state.Request.Content is GenericContent content && socketTuner.CanTune)
                        SetSocketTuningContent(state.Request, content);

                    var handler = handlerProvider.Obtain(connectionTimeout);

                    state.Response = await SocketsHandlerInvoker.Invoke(handler, state.Request, token).ConfigureAwait(false);

                    var responseCode = (ResponseCode)(int)state.Response.StatusCode;
                    var responseHeaders = ResponseHeadersConverter.Convert(state.Response);

                    if (request.Method == RequestMethods.Head)
                        return new Response(responseCode, headers: responseHeaders);

                    var bodyReadResult = await bodyReader.ReadAsync(state.Response, token).ConfigureAwait(false);

                    if (bodyReadResult.ErrorCode.HasValue)
                        return new Response(bodyReadResult.ErrorCode.Value);

                    if (bodyReadResult.Stream == null)
                        return new Response(responseCode, bodyReadResult.Content, responseHeaders);

                    state.PreventNextDispose();

                    return new Response(responseCode, null, responseHeaders, new DisposableBodyStream(bodyReadResult.Stream, state));
                }
            }
            catch (Exception error)
            {
                var errorResponse = errorHandler.TryHandle(request, error, token, connectionTimeout);
                if (errorResponse == null)
                    throw;

                return errorResponse;
            }
        }

        private void SetSocketTuningContent(HttpRequestMessage request, GenericContent content)
        {
            var headers = request.Content.Headers;
            request.Content = new SocketTuningContent(content, socketTuner, log);

            foreach (var header in headers)
                request.Content.Headers.Add(header.Key, header.Value);
        }
    }
}