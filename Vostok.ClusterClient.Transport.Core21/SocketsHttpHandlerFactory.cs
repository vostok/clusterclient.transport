using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Transport.Core21
{
    [UsedImplicitly]
    public static class SocketsHttpHandlerFactory
    {
        [UsedImplicitly]
        public static Type SocketHandlerType => typeof(SocketsHttpHandler);

        [UsedImplicitly]
        public static HttpMessageHandler Create(
            IWebProxy proxy,
            bool allowAutoRedirect,
            TimeSpan connectionTimeout,
            TimeSpan connectionIdleTimeout,
            TimeSpan connectionLifetime,
            int maxConnectionsPerEndpoint,
            X509Certificate2[] clientCertificates)
        {
            var handler = new SocketsHttpHandler
            {
                Proxy = proxy,
                UseProxy = proxy != null,
                ConnectTimeout = connectionTimeout,
                AllowAutoRedirect = allowAutoRedirect,
                PooledConnectionIdleTimeout = connectionIdleTimeout,
                PooledConnectionLifetime = connectionLifetime,
                MaxConnectionsPerServer = maxConnectionsPerEndpoint,
                AutomaticDecompression = DecompressionMethods.None,
                MaxResponseHeadersLength = 64 * 1024,
                MaxAutomaticRedirections = 3,
                UseCookies = false,
                SslOptions =
                {
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                    RemoteCertificateValidationCallback = (_, __, ___, ____) => true,
                }
            };

            if (clientCertificates != null)
                foreach (var cert in clientCertificates)
                    handler.SslOptions.ClientCertificates.Add(cert);

            return handler;
        }
    }
}
