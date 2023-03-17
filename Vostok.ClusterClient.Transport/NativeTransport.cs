using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Native;
using Vostok.Clusterclient.Transport.SystemNetHttp.BodyReading;
using Vostok.Clusterclient.Transport.SystemNetHttp.Header;
using Vostok.Clusterclient.Transport.SystemNetHttp.Helpers;
using Vostok.Clusterclient.Transport.SystemNetHttp.Messages;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport
{
    /// <summary>
    /// <para>A legacy ClusterClient transport for .NET Core 2.0. Internally uses <c>WinHttpHandler</c> on Windows and <c>CurlHandler</c> on Unix-like OS.</para>
    /// </summary>
    [PublicAPI]
    [Obsolete("Don't use this ITransport implementation on .NET Core 2.1 or later. Use SocketsTransport instead.")]
    public class NativeTransport : ITransport
    {
        private static readonly NativeTransportSettings DefaultSettings = new NativeTransportSettings();

        private readonly NativeTransportSettings settings;
        private readonly ILog log;

        private readonly Func<Request, TimeSpan?, CancellationToken, Task<Response>> sendAsyncDelegate;
        private readonly HttpClientProvider clientProvider;
        private readonly TimeoutProvider timeoutProvider;
        private readonly ErrorHandler errorHandler;
        private readonly BodyReader bodyReader;

        /// <inheritdoc cref="NativeTransport" />
        public NativeTransport([NotNull] ILog log)
            : this(DefaultSettings, log)
        {
        }

        /// <inheritdoc cref="NativeTransport" />
        public NativeTransport([NotNull] NativeTransportSettings settings, [NotNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log ?? throw new ArgumentNullException(nameof(log));

            sendAsyncDelegate = SendAsync;
            clientProvider = new HttpClientProvider(settings, this.log);
            timeoutProvider = new TimeoutProvider(settings.RequestAbortTimeout, this.log);
            errorHandler = new ErrorHandler(this.log);
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

                    var client = clientProvider.Obtain(connectionTimeout);

                    state.Response = await client.SendAsync(state.Request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

                    var responseCode = (ResponseCode)(int)state.Response.StatusCode;
                    var responseHeaders = ResponseHeadersConverter.Convert(state.Response);

                    if (request.Method == RequestMethods.Head)
                        return new Response(responseCode, headers: responseHeaders);

                    var bodyReadResult = await bodyReader.ReadAsync(state.Response, token).ConfigureAwait(false);

                    if (bodyReadResult.ErrorCode.HasValue)
                        return new Response(bodyReadResult.ErrorCode.Value, headers: responseHeaders);

                    if (bodyReadResult.Stream == null)
                        return new Response(responseCode, bodyReadResult.Content, responseHeaders);

                    state.PreventNextDispose();

                    return new Response(responseCode, null, responseHeaders, new DisposableBodyStream(bodyReadResult.Stream, state));
                }
            }
            catch (Exception error)
            {
                var errorResponse = errorHandler.TryHandle(request, error, token);
                if (errorResponse == null)
                    throw;

                return errorResponse;
            }
        }
    }
}