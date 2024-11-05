using System;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Vostok.Clusterclient.Transport.Sockets.HttpMessageHandlersCache;

internal readonly struct GlobalCacheKey
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
    public readonly bool TcpKeepAliveEnables;
    public readonly TimeSpan TcpKeepAliveInterval;
    public readonly TimeSpan TcpKeepAliveTime;

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
        DecompressionMethods decompressionMethods,
        bool tcpKeepAliveEnables,
        TimeSpan tcpKeepAliveInterval,
        TimeSpan tcpKeepAliveTime)
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
        TcpKeepAliveEnables = tcpKeepAliveEnables;
        TcpKeepAliveInterval = tcpKeepAliveInterval;
        TcpKeepAliveTime = tcpKeepAliveTime;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GlobalCacheKey Create(SocketsTransportSettings settings, TimeSpan? connectionTimeout)
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
            settings.DecompressionMethods,
            settings.TcpKeepAliveEnabled,
            settings.TcpKeepAliveInterval,
            settings.TcpKeepAliveTime);
}
