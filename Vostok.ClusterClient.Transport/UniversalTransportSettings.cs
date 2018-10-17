using System;

namespace Vostok.Clusterclient.Transport
{
    public class UniversalTransportSettings
    {
        public TimeSpan RequestAbortTimeout { get; set; } = TimeSpan.FromMilliseconds(250);

        public long? MaxResponseBodySize { get; set; } = null;

        public Predicate<long?> UseResponseStreaming { get; set; } = _ => false;

        public bool AllowAutoRedirect { get; set; } = false;

        public bool TcpKeepAliveEnabled { get; set; } = false;

        public TimeSpan TcpKeepAliveTime { get; set; } = TimeSpan.FromSeconds(3);

        public TimeSpan TcpKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}