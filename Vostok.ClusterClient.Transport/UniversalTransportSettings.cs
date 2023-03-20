using System;
using System.Net;
#if NETCOREAPP
using System.Net.Http;
#endif
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Transport
{
    /// <summary>
    /// A class that represents <see cref="UniversalTransport" /> settings.
    /// </summary>
    [PublicAPI]
    public class UniversalTransportSettings
    {
        /// <summary>
        /// How much time connection will be alive after last usage.
        /// </summary>
        public TimeSpan ConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// How much time client should wait for internal handler to return control after request cancellation.
        /// </summary>
        public TimeSpan RequestAbortTimeout { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Gets or sets an <see cref="IWebProxy" /> instance which will be used to send requests.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Max connections count to a single endpoint. When this limit is reached, requests get placed into a queue and wait for a free connection.
        /// </summary>
        public int MaxConnectionsPerEndpoint { get; set; } = 10 * 1000;

        /// <summary>
        /// Gets or sets the maximum response body size in bytes. This parameter doesn't affect content streaming.
        /// </summary>
        public long? MaxResponseBodySize { get; set; }

        /// <summary>
        /// Gets or sets the delegate that decides whether to use response streaming or not.
        /// </summary>
        public Predicate<long?> UseResponseStreaming { get; set; } = _ => false;

        /// <summary>
        /// Gets or sets a value that indicates whether the transport should follow HTTP redirection responses.
        /// </summary>
        public bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// Enables/disables TCP keep-alive mechanism. Currently only works in Windows.
        /// </summary>
        public bool TcpKeepAliveEnabled { get; set; }

        /// <summary>
        /// Gets or sets the duration between two keep-alive transmissions in idle condition.
        /// </summary>
        public TimeSpan TcpKeepAliveTime { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Gets or sets the duration between two successive keep-alive retransmissions than happen when acknowledgement to the previous
        /// keep-alive transmission is not received.
        /// </summary>
        public TimeSpan TcpKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets a delegate used to create response body buffers for given sizes.
        /// </summary>
        public Func<int, byte[]> BufferFactory { get; set; } = size => new byte[size];

        /// <summary>
        /// Gets or sets a list of client certificates for SSL connections.
        /// </summary>
        public X509Certificate2[] ClientCertificates { get; set; }

        /// <summary>
        /// Gets or sets a callback method to validate the server certificate.
        /// </summary>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; } = (sender, certificate, chain, errors) => true;

        /// <summary>
        /// Gets or sets credentials for Web client authentication.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the type of decompression that is used.
        /// </summary>
        public DecompressionMethods DecompressionMethods { get; set; }

#if NETCOREAPP
        /// <summary>
        /// Gets or sets the HTTP version. If not defined, the default version will be used depending on the framework version.
        /// </summary>
        public Version HttpVersion { get; set; } = null;
#endif

#if NET5_0_OR_GREATER
        /// <summary>
        /// Gets or sets the HTTP version policy. If not defined, the default policy will be used depending on the framework version.
        /// </summary>
        public HttpVersionPolicy? HttpVersionPolicy { get; set; } = null;
#endif

        [NotNull]
        public SocketsTransportSettings ToSocketsTransportSettings()
        {
            return new SocketsTransportSettings
            {
                AllowAutoRedirect = AllowAutoRedirect,
                ArpCacheWarmupEnabled = false,
                BufferFactory = BufferFactory,
                ClientCertificates = ClientCertificates,
                ConnectionIdleTimeout = ConnectionIdleTimeout,
                MaxConnectionsPerEndpoint = MaxConnectionsPerEndpoint,
                MaxResponseBodySize = MaxResponseBodySize,
                Proxy = Proxy,
                RequestAbortTimeout = RequestAbortTimeout,
                TcpKeepAliveEnabled = TcpKeepAliveEnabled,
                TcpKeepAliveInterval = TcpKeepAliveInterval,
                TcpKeepAliveTime = TcpKeepAliveTime,
                UseResponseStreaming = UseResponseStreaming,
                RemoteCertificateValidationCallback = RemoteCertificateValidationCallback,
                Credentials = Credentials,
                DecompressionMethods = DecompressionMethods,
#if NETCOREAPP
                HttpVersion = HttpVersion,
#endif
#if NET5_0_OR_GREATER
                HttpVersionPolicy = HttpVersionPolicy
#endif
            };
        }

        [NotNull]
        public WebRequestTransportSettings ToWebRequestTransportSettings()
        {
            return new WebRequestTransportSettings
            {
                AllowAutoRedirect = AllowAutoRedirect,
                ArpCacheWarmupEnabled = false,
                BufferFactory = BufferFactory,
                ClientCertificates = ClientCertificates,
                ConnectionIdleTimeout = ConnectionIdleTimeout,
                MaxConnectionsPerEndpoint = MaxConnectionsPerEndpoint,
                MaxResponseBodySize = MaxResponseBodySize,
                Pipelined = false,
                Proxy = Proxy,
                RequestAbortTimeout = RequestAbortTimeout,
                TcpKeepAliveEnabled = TcpKeepAliveEnabled,
                TcpKeepAliveInterval = TcpKeepAliveInterval,
                TcpKeepAliveTime = TcpKeepAliveTime,
                UseResponseStreaming = UseResponseStreaming,
                RemoteCertificateValidationCallback = RemoteCertificateValidationCallback,
                Credentials = Credentials,
                DecompressionMethods = DecompressionMethods
            };
        }

        [NotNull]
        public NativeTransportSettings ToNativeTransportSettings()
        {
            return new NativeTransportSettings
            {
                AllowAutoRedirect = AllowAutoRedirect,
                BufferFactory = BufferFactory,
                MaxConnectionsPerEndpoint = MaxConnectionsPerEndpoint,
                MaxResponseBodySize = MaxResponseBodySize,
                Proxy = Proxy,
                RequestAbortTimeout = RequestAbortTimeout,
                UseResponseStreaming = UseResponseStreaming,
                RemoteCertificateValidationCallback =
                    (message, certificate, chain, errors) => RemoteCertificateValidationCallback(message, certificate, chain, errors),
                Credentials = Credentials,
                DecompressionMethods = DecompressionMethods,
#if NETCOREAPP
                HttpVersion = HttpVersion,
#endif
#if NET5_0_OR_GREATER
                HttpVersionPolicy = HttpVersionPolicy
#endif
            };
        }
    }
}