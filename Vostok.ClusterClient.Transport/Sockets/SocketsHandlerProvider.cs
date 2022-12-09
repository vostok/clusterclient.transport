using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Commons.Collections;
using Vostok.Commons.Environment;

namespace Vostok.Clusterclient.Transport.Sockets
{
    internal class SocketsHandlerProvider
    {
        private const int GlobalCacheCapacity = 25;
        private const int LocalCacheCapacity = 3;

        // (iloktionov): Global cache is limited in size to prevent leaks resulting from random connection timeouts:
        private static readonly RecyclingBoundedCache<GlobalCacheKey, HttpMessageHandler> globalCache
            = new RecyclingBoundedCache<GlobalCacheKey, HttpMessageHandler>(GlobalCacheCapacity, GlobalCacheKeyComparer.Instance);

        // (iloktionov): Local cache isolates well-behaving clients from others who cause global cache purging:
        private readonly RecyclingBoundedCache<TimeSpan, HttpMessageHandler> localCache;
        private readonly Func<TimeSpan, HttpMessageHandler> localCacheFactory;

        public SocketsHandlerProvider(SocketsTransportSettings settings)
        {
            localCache = new RecyclingBoundedCache<TimeSpan, HttpMessageHandler>(LocalCacheCapacity);
            localCacheFactory = timeout => globalCache.Obtain(CreateKey(settings, timeout), CreateHandler);
        }

        public HttpMessageHandler Obtain(TimeSpan? connectionTimeout)
            => localCache.Obtain(connectionTimeout ?? Timeout.InfiniteTimeSpan, localCacheFactory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static GlobalCacheKey CreateKey(SocketsTransportSettings settings, TimeSpan? connectionTimeout)
            => new GlobalCacheKey(
                settings.Proxy,
                settings.AllowAutoRedirect,
                connectionTimeout ?? Timeout.InfiniteTimeSpan,
                settings.ConnectionIdleTimeout,
                settings.ConnectionLifetime,
                settings.MaxConnectionsPerEndpoint,
                settings.ClientCertificates,
                settings.RemoteCertificateValidationCallback,
                settings.Credentials,
                settings.DecompressionMethods);

        private static HttpMessageHandler CreateHandler(GlobalCacheKey key)
        {
            var handler = NetCore21Utils.CreateSocketsHandler(
                key.Proxy,
                key.AllowAutoRedirect,
                key.ConnectionTimeout,
                key.ConnectionIdleTimeout,
                key.ConnectionLifetime,
                key.MaxConnectionsPerEndpoint,
                key.ClientCertificates,
                key.RemoteCertificateValidationCallback,
                key.Credentials,
                key.DecompressionMethods);

            if (RuntimeDetector.IsDotNet50AndNewer)
                NetCore50Utils.TuneHandler(handler);

            return handler;
        }

        private readonly struct GlobalCacheKey
        {
            public readonly IWebProxy Proxy;
            public readonly bool AllowAutoRedirect;
            public readonly TimeSpan ConnectionTimeout;
            public readonly TimeSpan ConnectionIdleTimeout;
            public readonly TimeSpan ConnectionLifetime;
            public readonly int MaxConnectionsPerEndpoint;
            public readonly X509Certificate2[] ClientCertificates;
            public readonly RemoteCertificateValidationCallback RemoteCertificateValidationCallback;
            public readonly ICredentials Credentials;
            public readonly DecompressionMethods DecompressionMethods;

            public GlobalCacheKey(
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
                Proxy = proxy;
                AllowAutoRedirect = allowAutoRedirect;
                ConnectionTimeout = connectionTimeout;
                ConnectionIdleTimeout = connectionIdleTimeout;
                ConnectionLifetime = connectionLifetime;
                MaxConnectionsPerEndpoint = maxConnectionsPerEndpoint;
                ClientCertificates = clientCertificates;
                RemoteCertificateValidationCallback = remoteCertificateValidationCallback;
                Credentials = credentials;
                DecompressionMethods = decompressionMethods;
            }
        }

        private class GlobalCacheKeyComparer : IEqualityComparer<GlobalCacheKey>
        {
            public static readonly GlobalCacheKeyComparer Instance = new GlobalCacheKeyComparer();

            public bool Equals(GlobalCacheKey x, GlobalCacheKey y)
            {
                return
                    ReferenceEquals(x.Proxy, y.Proxy) &&
                    ReferenceEquals(x.ClientCertificates, y.ClientCertificates) &&
                    x.AllowAutoRedirect == y.AllowAutoRedirect &&
                    x.ConnectionTimeout == y.ConnectionTimeout &&
                    x.ConnectionIdleTimeout == y.ConnectionIdleTimeout &&
                    x.ConnectionLifetime == y.ConnectionLifetime &&
                    x.MaxConnectionsPerEndpoint == y.MaxConnectionsPerEndpoint &&
                    x.RemoteCertificateValidationCallback == y.RemoteCertificateValidationCallback &&
                    Equals(x.Credentials, y.Credentials) &&
                    x.DecompressionMethods == y.DecompressionMethods;
            }

            public int GetHashCode(GlobalCacheKey key)
            {
                unchecked
                {
                    var hashCode = key.Proxy != null ? key.Proxy.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ key.AllowAutoRedirect.GetHashCode();
                    hashCode = (hashCode * 397) ^ key.ConnectionTimeout.GetHashCode();
                    hashCode = (hashCode * 397) ^ key.ConnectionIdleTimeout.GetHashCode();
                    hashCode = (hashCode * 397) ^ key.ConnectionLifetime.GetHashCode();
                    hashCode = (hashCode * 397) ^ key.MaxConnectionsPerEndpoint;
                    hashCode = (hashCode * 397) ^ (key.ClientCertificates != null ? key.ClientCertificates.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (key.RemoteCertificateValidationCallback != null ? key.RemoteCertificateValidationCallback.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (key.Credentials != null ? key.Credentials.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ key.DecompressionMethods.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}