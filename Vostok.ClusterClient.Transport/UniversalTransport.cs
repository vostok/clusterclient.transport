using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Universal;
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

        private readonly object sync = new object();
        private readonly UniversalTransportSettings settings;
        private readonly ILog log;

        private volatile ITransport implementation;

        /// <inheritdoc cref="UniversalTransport" />
        public UniversalTransport([NotNull] UniversalTransportSettings settings, [NotNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <inheritdoc cref="UniversalTransport" />
        public UniversalTransport([NotNull] ILog log)
            : this(DefaultSettings, log)
        {
        }

        /// <inheritdoc />
        public TransportCapabilities Capabilities 
            => ObtainImplementation().Capabilities;

        /// <inheritdoc />
        public Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
            => ObtainImplementation().SendAsync(request, connectionTimeout, timeout, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ITransport ObtainImplementation()
        {
            if (implementation != null)
                return implementation;

            lock (sync)
            {
                if (implementation != null)
                    return implementation;

                return implementation = TransportFactory.Create(settings, log);
            }
        }
    }
}
