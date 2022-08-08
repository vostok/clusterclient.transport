using System;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Native
{
    internal class ErrorHandler
    {
        private readonly ILog log;

        public ErrorHandler(ILog log)
            => this.log = log;

        [CanBeNull]
        public Response TryHandle(Request request, Exception error, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return Responses.Canceled;

            switch (error)
            {
                case StreamAlreadyUsedException _:
                case ContentAlreadyUsedException _:
                    return null;

                case UserStreamException _:
                    LogUserStreamFailure(error);
                    return Responses.StreamInputFailure;

                case UserContentProducerException _:
                    LogUserContentProducerFailure(request, error);
                    return Responses.ContentInputFailure;

                case BodySendException _:
                    log.Error(error);
                    return Responses.SendFailure;

                case HttpRequestException httpError:

                    switch (httpError.InnerException)
                    {
                        case OperationCanceledException _:
                            LogRequestTimeout(request);
                            return Responses.Timeout;

                        case Win32Exception win32Error:
                            var response = WinHttpErrorsHandler.Handle(win32Error);
                            if (response.Code == ResponseCode.ConnectFailure)
                            {
                                LogConnectionFailure(request, win32Error);
                            }
                            else
                            {
                                LogWin32Error(request, win32Error);
                            }

                            return response;
                        
                        case AuthenticationException authenticationException:
                            LogConnectionFailure(request, authenticationException);
                            return Responses.BadRequest;

                        default:
                            if (CurlExceptionHelper.IsCurlException(httpError.InnerException, out var curlCode))
                            {
                                if (CurlExceptionHelper.IsConnectionFailure(curlCode))
                                {
                                    LogConnectionFailure(request, error);
                                    return Responses.ConnectFailure;
                                }

                                LogCurlError(request, error, curlCode);
                                return Responses.UnknownFailure;
                            }

                            break;
                    }

                    break;
            }

            LogUnknownException(error);
            return Responses.UnknownFailure;
        }

        private void LogConnectionFailure(Request request, Exception error)
            => log.Warn(error, "Connection failure. Target = {Target}.", request.Url.Authority);

        private void LogRequestTimeout(Request request)
            => log.Warn("Request timed out. Target = {Target}.", request.Url.Authority);

        private void LogWin32Error(Request request, Win32Exception error)
            => log.Warn(error, "WinAPI error with code {ErrorCode} occured while sending request to {Target}.", error.NativeErrorCode, request.Url.Authority);

        private void LogCurlError(Request request, Exception error, CurlCode code)
            => log.Warn(error, "CURL error with code {ErrorCode} occured while sending request to {Target}.", code, request.Url.Authority);

        private void LogUserStreamFailure(Exception error)
            => log.Error(error, "Failed to read from user-provided request body stream");

        private void LogUserContentProducerFailure(Request request, Exception error)
            => log.Warn(error, "Failed to read request body from user-provided content producer while sending request to '{Target}'.", request.Url.Authority);

        private void LogUnknownException(Exception error)
            => log.Error(error, "Unknown exception in sending request.");
    }
}