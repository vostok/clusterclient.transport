using System.Collections.Generic;

namespace Vostok.Clusterclient.Transport.Sockets.HttpMessageHandlersCache;

// NOTE: используется в сингуляре (WS проект) через Include .cs файлов
internal class GlobalCacheKeyComparer : IEqualityComparer<GlobalCacheKey>
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
            x.DecompressionMethods == y.DecompressionMethods &&
            x.TcpKeepAliveEnables == y.TcpKeepAliveEnables &&
            x.TcpKeepAliveInterval == y.TcpKeepAliveInterval &&
            x.TcpKeepAliveTime == y.TcpKeepAliveTime;
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
            hashCode = (hashCode * 397) ^ key.TcpKeepAliveEnables.GetHashCode();
            hashCode = (hashCode * 397) ^ key.TcpKeepAliveInterval.GetHashCode();
            hashCode = (hashCode * 397) ^ key.TcpKeepAliveTime.GetHashCode();
            return hashCode;
        }
    }
}