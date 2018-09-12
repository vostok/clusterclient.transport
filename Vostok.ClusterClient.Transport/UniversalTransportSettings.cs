using System;

namespace Vostok.ClusterClient.Transport
{
    public class UniversalTransportSettings
    {
        public bool Pipelined { get; set; } = true;

        public bool FixThreadPoolProblems { get; set; } = true;

        public int ConnectionAttempts { get; set; } = 2;

        public TimeSpan? ConnectionTimeout { get; set; } = TimeSpan.FromMilliseconds(750);

        public TimeSpan RequestAbortTimeout { get; set; } = TimeSpan.FromMilliseconds(250);

        public long? MaxResponseBodySize { get; set; } = null;

        public Predicate<long?> UseResponseStreaming { get; set; } = _ => false;

        public bool AllowAutoRedirect { get; set; } = false;

        public bool TcpKeepAliveEnabled { get; set; } = false;

        public TimeSpan TcpKeepAliveTime { get; set; } = TimeSpan.FromSeconds(3);

        public TimeSpan TcpKeepAlivePeriod { get; set; } = TimeSpan.FromSeconds(1);
    }
}