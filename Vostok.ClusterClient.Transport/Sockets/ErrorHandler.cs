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
                    var connectionError = TryDetectConnectionError(error);
                    if (connectionError != null)
                    {
                        LogConnectionFailure(request, connectionError, connectionTimeout);
                        return Responses.ConnectFailure;
                    }

                    break;
            }

            LogUnknownException(request, error);
            return Responses.UnknownFailure;
        }

        [CanBeNull]
        private static Exception TryDetectConnectionError(Exception error)
        {
            while (true)
            {
                if (error == null)
                    return null;

                if (error is OperationCanceledException)
                    return error;

                if (error is SocketException socketError && IsConnectionFailure(socketError.SocketErrorCode))
                    return error;

                if (error is IOException ioError && ioError.InnerException == null)
                    return error;

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

        private void LogConnectionFailure(Request request, Exception error, TimeSpan? connectionTimeout)
        {
            if (error is OperationCanceledException)
            {
                log.Warn("Connection attempt timed out. Target = '{Target}'. Timeout = {ConnectionTimeout}.", new
                {
                    Target = request.Url.Authority,
                    ConnectionTimeout = FormatConnectionTimeout(connectionTimeout),
                    ConnectionTimeoutMs = connectionTimeout?.TotalMilliseconds ?? 0d
                });
                return;
            }

            if (error is SocketException socketError)
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

            log.Warn(error, "Connection failure. Target = '{Target}'.", request.Url.Authority);
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
