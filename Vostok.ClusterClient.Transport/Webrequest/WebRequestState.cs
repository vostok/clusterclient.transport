using System;
using System.IO;
using System.Net;
using System.Threading;
using Vostok.Commons.Time;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal class WebRequestState : IDisposable
    {
        private readonly TimeBudget budget;
        private int cancellationState;
        private int disposeBarrier;

        public WebRequestState(TimeSpan timeout)
            => budget = TimeBudget.StartNew(timeout, 5.Milliseconds());

        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }

        public Stream RequestStream { get; set; }
        public Stream ResponseStream { get; set; }

        public byte[] BodyBuffer { get; set; }
        public int BodyBufferLength { get; set; }

        public MemoryStream BodyStream { get; set; }
        public bool ReturnStreamDirectly { get; set; }

        public TimeSpan TimeRemaining => budget.Remaining;

        public bool RequestCanceled => cancellationState > 0;

        public void CancelRequest()
        {
            Interlocked.Exchange(ref cancellationState, 1);

            CancelRequestAttempt();
        }

        public void CancelRequestAttempt()
        {
            if (Request != null)
                try
                {
                    Request.Abort();
                }
                catch
                {
                    // ignored
                }
        }

        public void PreventNextDispose()
        {
            Interlocked.Exchange(ref disposeBarrier, 1);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposeBarrier, 0) > 0)
                return;

            CloseRequestStream();
            CloseResponseStream();
        }

        public void CloseRequestStream()
        {
            if (RequestStream != null)
                try
                {
                    RequestStream.Close();
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    RequestStream = null;
                }
        }

        private void CloseResponseStream()
        {
            if (ResponseStream != null)
                try
                {
                    ResponseStream.Close();
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    ResponseStream = null;
                }
        }
    }
}
