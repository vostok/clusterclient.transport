using System;

namespace Vostok.Clusterclient.Transport
{
    public class UniversalTransportSettings
    {
        /// <summary>
        ///     How much time client should wait for internal handler return after request cancellation.
        /// </summary>
        public TimeSpan RequestAbortTimeout { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        ///     Gets or sets the maximum response body size in bytes. This parameter doesn't affect content streaming.
        /// </summary>
        public long? MaxResponseBodySize { get; set; } = null;

        /// <summary>
        ///     Gets or sets the delegate that decide use response streaming or not.
        /// </summary>
        public Predicate<long?> UseResponseStreaming { get; set; } = _ => false;

        /// <summary>
        ///     Gets or sets a value that indicates whether the transport should follow HTTP redirection responses.
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = false;

        /// <summary>
        ///     Enables TCP keep alive.
        /// </summary>
        public bool TcpKeepAliveEnabled { get; set; } = false;

        /// <summary>
        ///     Gets or sets the duration between two keepalive transmissions in idle condition.
        /// </summary>
        public TimeSpan TcpKeepAliveTime { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        ///     Gets ot sets the duration between two successive keepalive retransmissions, if acknowledgement to the previous
        ///     keepalive
        ///     transmission is not received.
        /// </summary>
        public TimeSpan TcpKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(1);

        internal UniversalTransportSettings Clone()
        {
            return new UniversalTransportSettings
            {
                TcpKeepAliveEnabled = TcpKeepAliveEnabled,
                TcpKeepAliveInterval = TcpKeepAliveInterval,
                TcpKeepAliveTime = TcpKeepAliveTime,
                AllowAutoRedirect = AllowAutoRedirect,
                RequestAbortTimeout = RequestAbortTimeout,
                UseResponseStreaming = UseResponseStreaming,
                MaxResponseBodySize = MaxResponseBodySize
            };
        }
    }
}