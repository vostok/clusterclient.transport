using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport
{
    /// <summary>
    /// Universal transport which internally uses different runtime-dependent implementations.
    /// </summary>
    [PublicAPI]
    public class UniversalTransport : ITransport
    {
        private static readonly UniversalTransportSettings DefaultSettings = new UniversalTransportSettings();

        private readonly UniversalTransportSettings settings;
        private readonly ILog log;
        private readonly object sync = new object();
        private volatile ITransport implementation;

        /// <inheritdoc cref="UniversalTransport" />
        public UniversalTransport([NotNull] UniversalTransportSettings settings, [NotNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<UniversalTransport>();
        }

        /// <inheritdoc cref="UniversalTransport" />
        public UniversalTransport([NotNull] ILog log)
            : this(DefaultSettings, log)
        {
        }

        /// <inheritdoc />
        public TransportCapabilities Capabilities => implementation.Capabilities;

        /// <inheritdoc />
        public Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
        {
            // ReSharper disable once InvertIf
            if (implementation == null)
            {
                lock (sync)
                {
                    if (implementation == null)
                        implementation = TransportFactory.Create(settings, log);
                }
            }

            return implementation.SendAsync(request, connectionTimeout, timeout, cancellationToken);
        }
    }
}