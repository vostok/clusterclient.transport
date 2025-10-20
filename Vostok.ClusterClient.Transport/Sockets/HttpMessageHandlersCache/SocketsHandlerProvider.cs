using System;
using System.Net.Http;
using System.Threading;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Commons.Collections;
using Vostok.Commons.Environment;

namespace Vostok.Clusterclient.Transport.Sockets.HttpMessageHandlersCache
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
            localCacheFactory = timeout => globalCache.Obtain(GlobalCacheKey.Create(settings, timeout), CreateHandler);
        }

        public HttpMessageHandler Obtain(TimeSpan? connectionTimeout)
            => localCache.Obtain(connectionTimeout ?? Timeout.InfiniteTimeSpan, localCacheFactory);

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
                NetCore50Utils.TuneHandler(handler, key.TcpKeepAliveEnables, key.TcpKeepAliveInterval, key.TcpKeepAliveTime, key.EnableMultipleHttp2Connections);

            if (RuntimeDetector.IsDotNet60AndNewer)
                NetCore60Utils.TuneHandler(handler);

            return handler;
        }
    }
}