using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal class ConnectTimeLimiter
    {
        private readonly ILog log;

        public ConnectTimeLimiter(ILog log)
            => this.log = log;

        public Task<HttpActionStatus> LimitConnectTime(Task<HttpActionStatus> mainTask, Request request, WebRequestState state, TimeSpan? connectTimeout)
        {
            if (connectTimeout == null)
                return mainTask;

            if (!ConnectTimeoutHelper.CanCheckSocket)
                return mainTask;

            if (state.TimeRemaining < connectTimeout.Value)
                return mainTask;

            if (request.Url.IsLoopback)
                return mainTask;

            if (ConnectTimeoutHelper.IsSocketConnected(state.Request, log))
                return mainTask;

            return LimitConnectTimeInternal(mainTask, request, state, connectTimeout.Value);
        }

        private async Task<HttpActionStatus> LimitConnectTimeInternal(Task<HttpActionStatus> mainTask, Request request, WebRequestState state, TimeSpan connectTimeout)
        {
            using (var timeoutCancellation = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(mainTask, Task.Delay(connectTimeout, timeoutCancellation.Token)).ConfigureAwait(false);
                if (completedTask is Task<HttpActionStatus> taskWithResult)
                {
                    timeoutCancellation.Cancel();
                    return taskWithResult.GetAwaiter().GetResult();
                }

                if (!ConnectTimeoutHelper.IsSocketConnected(state.Request, log))
                {
                    state.CancelRequestAttempt();
                    LogConnectionFailure(request, new WebException($"Connection attempt timed out. Timeout = {connectTimeout.ToPrettyString()}.", WebExceptionStatus.ConnectFailure));
                    return HttpActionStatus.ConnectionFailure;
                }

                return await mainTask.ConfigureAwait(false);
            }
        }

        private void LogConnectionFailure(Request request, WebException error)
        {
            log.Warn(error.InnerException ?? error,
                "Connection failure. Target = {Target}. Status = {Status}.",
                request.Url.Authority, error.Status);
        }
    }
}
