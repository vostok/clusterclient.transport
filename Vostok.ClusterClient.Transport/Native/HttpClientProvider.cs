using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Vostok.Commons.Collections;
using Vostok.Logging.Abstractions;
using RemoteCertificateValidationCallback = System.Func<System.Net.Http.HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool>;

namespace Vostok.Clusterclient.Transport.Native
{
    internal class HttpClientProvider
    {
        private const int GlobalCacheCapacity = 25;

        private static readonly RecyclingBoundedCache<CacheKey, HttpClient> Cache
            = new RecyclingBoundedCache<CacheKey, HttpClient>(GlobalCacheCapacity, CacheKeyComparer.Instance);

        private readonly NativeTransportSettings settings;
        private readonly ILog log;

        public HttpClientProvider(NativeTransportSettings settings, ILog log)
        {
            this.settings = settings;
            this.log = log;
        }

        public HttpClient Obtain(TimeSpan? connectionTimeout)
            => Cache.Obtain(CreateCacheKey(connectionTimeout), key => new HttpClient(HttpClientHandlerFactory.Build(settings, key.ConnectionTimeout, log), true));

        private CacheKey CreateCacheKey(TimeSpan? connectionTimeout)
            => new CacheKey(settings.Proxy, connectionTimeout, settings.AllowAutoRedirect, settings.MaxConnectionsPerEndpoint, settings.RemoteCertificateValidationCallback);

        private struct CacheKey
        {
            public readonly IWebProxy Proxy;
            public readonly TimeSpan? ConnectionTimeout;
            public readonly bool AllowAutoRedirects;
            public readonly int MaxConnectionsPerEndpoint;
            public readonly RemoteCertificateValidationCallback RemoteCertificateValidationCallback;

            public CacheKey(IWebProxy proxy, TimeSpan? connectionTimeout, bool allowAutoRedirects, int maxConnectionsPerEndpoint, RemoteCertificateValidationCallback remoteCertificateValidationCallback)
            {
                Proxy = proxy;
                ConnectionTimeout = connectionTimeout;
                AllowAutoRedirects = allowAutoRedirects;
                MaxConnectionsPerEndpoint = maxConnectionsPerEndpoint;
                RemoteCertificateValidationCallback = remoteCertificateValidationCallback;
            }
        }

        private class CacheKeyComparer : IEqualityComparer<CacheKey>
        {
            public static readonly CacheKeyComparer Instance = new CacheKeyComparer();

            public bool Equals(CacheKey x, CacheKey y)
                => ReferenceEquals(x.Proxy, y.Proxy) &&
                   x.ConnectionTimeout == y.ConnectionTimeout &&
                   x.AllowAutoRedirects == y.AllowAutoRedirects &&
                   x.MaxConnectionsPerEndpoint == y.MaxConnectionsPerEndpoint &&
                   x.RemoteCertificateValidationCallback == y.RemoteCertificateValidationCallback;

            public int GetHashCode(CacheKey item)
            {
                unchecked
                {
                    var hash = item.Proxy?.GetHashCode() ?? 0;

                    hash = (hash * 397) ^ item.ConnectionTimeout.GetHashCode();
                    hash = (hash * 397) ^ item.AllowAutoRedirects.GetHashCode();
                    hash = (hash * 397) ^ item.MaxConnectionsPerEndpoint;
                    hash = (hash * 397) ^ (item.RemoteCertificateValidationCallback != null ? item.RemoteCertificateValidationCallback.GetHashCode() : 0);

                    return hash;
                }
            }
        }
    }
}