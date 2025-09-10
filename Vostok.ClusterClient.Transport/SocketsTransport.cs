using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Clusterclient.Transport.Sockets.HttpMessageHandlersCache;
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

        private readonly Func<Request, TimeSpan?, CancellationToken, Task<Response>> sendAsyncDelegate;
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

            sendAsyncDelegate = SendAsync;
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
               TransportCapabilities.ResponseStreaming | 
               TransportCapabilities.ResponseTrailers;

        /// <inheritdoc />
        public Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken token)
            => timeoutProvider.SendWithTimeoutAsync(sendAsyncDelegate, request, connectionTimeout, timeout, token);

        private async Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, CancellationToken token)
        {
            try
            {
                using (var state = new DisposableState())
                {
                    state.Request = RequestMessageFactory.Create(request, token, log);
#if NETCOREAPP
                    //(deniaa): We cant set default HTTP version by ourselves.
                    //Netcoreapp21 has a default version of HTTP 2.0.
                    //NetFW* and Net6 has a default version of HTTP 1.1.
                    //So let framework choose the default version on its own if the user has not passed a specific version.
                    if (settings.HttpVersion != null)
                        state.Request.Version = settings.HttpVersion;
#endif
#if NET5_0_OR_GREATER
                    if (settings.HttpVersionPolicy.HasValue)
                        state.Request.VersionPolicy = settings.HttpVersionPolicy.Value;
#endif

#if !NET5_0_OR_GREATER
                    // Due to significant changes in System.Net.Http.HttpConnection class SocketTuningContent can't work starting from .Net7.
                    // But starting from .Net5 HttpMessageHandler has a special api for setting TcpKeepAlive options. See in NetCore50Utils.TuneHandler and SocketsHttpHandlerTuner.
                    if (state.Request.Content is GenericContent content && socketTuner.CanTune)
                        state.Request.Content = new SocketTuningContent(content, socketTuner, log);
#endif

#if NET5_0_OR_GREATER
                    settings.HeadersModifier?.Invoke(state.Request.Headers, state.Request.Content?.Headers);
#endif
                    
                    var handler = handlerProvider.Obtain(connectionTimeout);

                    state.Response = await SocketsHandlerInvoker.Invoke(handler, state.Request, token).ConfigureAwait(false);

                    var responseCode = (ResponseCode)(int)state.Response.StatusCode;
                    var responseHeaders = ResponseHeadersConverter.Convert(state.Response);

                    if (request.Method == RequestMethods.Head)
                        return new Response(responseCode, headers: responseHeaders);

                    var bodyReadResult = await bodyReader.ReadAsync(state.Response, token).ConfigureAwait(false);

                    if (bodyReadResult.ErrorCode.HasValue)
                        return new Response(bodyReadResult.ErrorCode.Value, headers: responseHeaders);

                    if (bodyReadResult.Stream == null)
                    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                        // note(d.khrustalev): мы создаём инстанс headers здесь, потому что к моменту вызова колбэка state может быть недоступен
                        var trailers = ResponseHeadersConverter.Convert(state.Response.TrailingHeaders);
#endif
                        return new Response(responseCode,
                            bodyReadResult.Content,
                            responseHeaders)
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                            .WithTrailersCallback(() => trailers)
#endif
                        ;
                    }

                    state.PreventNextDispose();

                    return new Response(responseCode, null, responseHeaders, new DisposableBodyStream(bodyReadResult.Stream, state))
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                        .WithTrailersCallback(() => ResponseHeadersConverter.Convert(state.Response.TrailingHeaders))
#endif
                        ;
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
    }
}