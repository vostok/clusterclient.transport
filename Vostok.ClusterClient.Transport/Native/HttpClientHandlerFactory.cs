using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Native
{
    internal static class HttpClientHandlerFactory
    {
        private static readonly object Sync = new object();
        private static volatile Func<HttpClientHandler> handlerFactory;

        public static HttpClientHandler Build(NativeTransportSettings settings, TimeSpan? connectionTimeout, ILog log)
        {
            EnsureInitialized(log);

            var handler = handlerFactory();

            handler.AllowAutoRedirect = settings.AllowAutoRedirect;
            handler.AutomaticDecompression = settings.DecompressionMethods;

            handler.MaxAutomaticRedirections = 3;
            handler.MaxConnectionsPerServer = settings.MaxConnectionsPerEndpoint;
            handler.MaxResponseHeadersLength = 64 * 1024;

            handler.Proxy = settings.Proxy;
            handler.UseProxy = settings.Proxy != null;
            handler.UseCookies = false;
            handler.UseDefaultCredentials = false;
            handler.PreAuthenticate = false;

            handler.ServerCertificateCustomValidationCallback = settings.RemoteCertificateValidationCallback;
            handler.Credentials = settings.Credentials;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WinHttpHandlerTuner.Tune(handler, connectionTimeout, log);

            return handler;
        }

        private static void EnsureInitialized(ILog log)
        {
            if (handlerFactory != null)
                return;

            lock (Sync)
            {
                if (handlerFactory != null)
                    return;

                handlerFactory = BuildHandler(log);
            }
        }

        private static Func<HttpClientHandler> BuildHandler(ILog log)
        {
            try
            {
                var handlerType = typeof(HttpClientHandler);

                var ctor = handlerType
                    .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(bool)}, null);

                if (ctor == null)
                    return () => new HttpClientHandler();

                return Expression.Lambda<Func<HttpClientHandler>>(Expression.New(ctor, Expression.Constant(false))).Compile();
            }
            catch (Exception error)
            {
                log.ForContext(typeof(HttpClientHandlerFactory)).Warn(error);

                return () => new HttpClientHandler();
            }
        }
    }
}