using System;
using System.Linq.Expressions;
using System.Net;
using Vostok.Commons.Environment;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal static class ConnectTimeoutHelper
    {
        private static readonly object Sync = new object();

        private static volatile bool canCheckSocket = true;

        private static Func<HttpWebRequest, bool> isSocketConnected;

        public static bool CanCheckSocket => canCheckSocket;

        public static bool IsSocketConnected(HttpWebRequest request, ILog log)
        {
            Initialize(log);

            if (!canCheckSocket)
                return true;

            try
            {
                return isSocketConnected(request);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (Exception error)
            {
                canCheckSocket = false;

                WrapLog(log).Error(error, "Failed to check socket connection");
            }

            return true;
        }

        private static void Initialize(ILog log)
        {
            if (isSocketConnected != null || !canCheckSocket)
                return;

            Exception savedError = null;

            lock (Sync)
            {
                if (isSocketConnected != null || !canCheckSocket)
                    return;

                try
                {
                    if (RuntimeDetector.IsDotNetFramework)
                    {
                        isSocketConnected = BuildSocketConnectedChecker();
                    }
                    else
                    {
                        isSocketConnected = _ => true;
                        canCheckSocket = false;
                    }
                }
                catch (Exception error)
                {
                    canCheckSocket = false;
                    savedError = error;
                }
            }

            if (savedError != null)
                WrapLog(log).Error(savedError, "Failed to build connection checker lambda");
        }

        // Builds the following lambda:
        // (HttpWebRequest request) => request._SubmitWriteStream != null && request._SubmitWriteStream.InternalSocket != null && request._SubmitWriteStream.InternalSocket.Connected
        private static Func<HttpWebRequest, bool> BuildSocketConnectedChecker()
        {
            var request = Expression.Parameter(typeof(HttpWebRequest));

            var stream = Expression.Field(request, "_SubmitWriteStream");
            var socket = Expression.Property(stream, "InternalSocket");
            var isConnected = Expression.Property(socket, "Connected");

            var body = Expression.AndAlso(
                Expression.ReferenceNotEqual(stream, Expression.Constant(null)),
                Expression.AndAlso(
                    Expression.ReferenceNotEqual(socket, Expression.Constant(null)),
                    isConnected));

            return Expression.Lambda<Func<HttpWebRequest, bool>>(body, request).Compile();
        }

        private static ILog WrapLog(ILog log)
            => log.ForContext(typeof(ConnectTimeoutHelper).Name);
    }
}
