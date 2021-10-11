using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Transport
{
    /// <summary>
    /// A class that represents <see cref="NativeTransport"/> settings.
    /// </summary>
    [PublicAPI]
    public class NativeTransportSettings
    {
        /// <summary>
        /// How much time client should wait for internal handler to return control after request cancellation.
        /// </summary>
        public TimeSpan RequestAbortTimeout { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Gets or sets an <see cref="IWebProxy" /> instance which will be used to send requests.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Max connections count to single endpoint. When this limit is reached, requests get placed into queue and wait for a free connection.
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
        /// Gets or sets a delegate used to create response body buffers for given sizes.
        /// </summary>
        public Func<int, byte[]> BufferFactory { get; set; } = size => new byte[size];

        /// <summary>
        /// Gets or sets a callback method to validate the server certificate.
        /// </summary>
        public Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> RemoteCertificateValidationCallback { get; set; } = (sender, certificate, chain, errors) => true;
    }
}