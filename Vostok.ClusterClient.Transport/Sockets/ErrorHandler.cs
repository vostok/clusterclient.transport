using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Sockets
{
    internal class ErrorHandler
    {
        private readonly ILog log;

        public ErrorHandler(ILog log)
        {
            this.log = log;
        }

        [CanBeNull]
        public Response TryHandle(Request request, Exception error, CancellationToken token, TimeSpan? connectionTimeout)
        {
            if (token.IsCancellationRequested)
                return Responses.Canceled;

            switch (error)
            {
                case StreamAlreadyUsedException _:
                    return null;

                case UserStreamException _:
                    LogUserStreamFailure(request, error);
                    return Responses.StreamInputFailure;

                case BodySendException _:
                    LogBodySendFailure(request, error);
                    return Responses.SendFailure;

                default:
                    var (connectionErrorResponse, connectionError) = TryClassifyConnectionError(error);
                    if (connectionErrorResponse != null)
                    {
                        LogConnectionFailure(request, connectionError, connectionTimeout);
                        return connectionErrorResponse;
                    }

                    break;
            }

            LogUnknownException(request, error);
            return Responses.UnknownFailure;
        }

        private static (Response response, Exception connectionError) TryClassifyConnectionError(Exception error)
        {
            while (true)
            {
                if (error == null)
                    return (response: null, connectionError: null);

                if (error is OperationCanceledException)
                    return (Responses.ConnectFailure, connectionError: error);

                if (error is SocketException socketError && IsConnectionFailure(socketError.SocketErrorCode))
                    return (Responses.ConnectFailure, connectionError: socketError);

                // todo (avk, 24.12.2020): 'IOException with no InnerException' also happens on connection establishment and corresponding request can be safely retried
                if (error is IOException ioError && ioError.InnerException == null)
                    return (Responses.ReceiveFailure, connectionError: ioError);

                error = error.InnerException;
            }
        }

        private static bool IsConnectionFailure(SocketError code)
        {
            switch (code)
            {
                case SocketError.HostDown:
                case SocketError.HostNotFound:
                case SocketError.HostUnreachable:

                case SocketError.NetworkDown:
                case SocketError.NetworkUnreachable:

                case SocketError.AddressNotAvailable:
                case SocketError.AddressAlreadyInUse:

                case SocketError.ConnectionRefused:
                case SocketError.ConnectionAborted:
                case SocketError.ConnectionReset:

                case SocketError.TimedOut:
                case SocketError.TryAgain:
                case SocketError.SystemNotReady:
                case SocketError.TooManyOpenSockets:
                case SocketError.NoBufferSpaceAvailable:
                case SocketError.DestinationAddressRequired:
                    return true;

                default:
                    return false;
            }
        }

        private void LogConnectionFailure(Request request, Exception connectionError, TimeSpan? connectionTimeout)
        {
            if (connectionError is OperationCanceledException)
            {
                log.Warn("Connection attempt timed out. Target = '{Target}'. Timeout = {ConnectionTimeout}.", new
                {
                    Target = request.Url.Authority,
                    ConnectionTimeout = FormatConnectionTimeout(connectionTimeout),
                    ConnectionTimeoutMs = connectionTimeout?.TotalMilliseconds ?? 0d
                });
                return;
            }

            if (connectionError is SocketException socketError)
            {
                log.Warn("Connection failure. Target = '{Target}'. Socket code = {SocketErrorCode}. Timeout = {ConnectionTimeout}.", new
                {
                    Target = request.Url.Authority,
                    socketError.SocketErrorCode,
                    ConnectionTimeout = FormatConnectionTimeout(connectionTimeout),
                    ConnectionTimeoutMs = connectionTimeout?.TotalMilliseconds ?? 0d
                });
                return;
            }

            log.Warn(connectionError, "Connection failure. Target = '{Target}'.", request.Url.Authority);
        }

        private void LogUserStreamFailure(Request request, Exception error)
            => log.Warn(error, "Failed to read from user-provided request body stream while sending request to '{Target}'.", request.Url.Authority);

        private void LogBodySendFailure(Request request, Exception error)
            => log.Warn(error, "Failed to send request body to '{Target}'.", request.Url.Authority);

        private void LogUnknownException(Request request, Exception error)
            => log.Error(error, "Unknown transport exception has occurred while sending request to '{Target}'.", request.Url.Authority);

        private static string FormatConnectionTimeout(TimeSpan? timeout)
            => timeout.HasValue ? timeout.Value.ToPrettyString() : "none";
    }
}
