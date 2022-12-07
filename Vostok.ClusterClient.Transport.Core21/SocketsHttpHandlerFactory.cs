using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
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
            X509Certificate2[] clientCertificates,
            RemoteCertificateValidationCallback remoteCertificateValidationCallback,
            ICredentials credentials,
            DecompressionMethods decompressionMethods)
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
                AutomaticDecompression = decompressionMethods,
                MaxResponseHeadersLength = 64 * 1024,
                MaxAutomaticRedirections = 3,
                UseCookies = false,
                SslOptions =
                {
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                    RemoteCertificateValidationCallback = remoteCertificateValidationCallback
                },
                Credentials = credentials
            };

            if (clientCertificates != null)
                handler.SslOptions.ClientCertificates = new X509Certificate2Collection(clientCertificates);

            return handler;
        }
    }
}