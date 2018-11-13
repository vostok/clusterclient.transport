using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport
{
    /// <inheritdoc />
    /// <summary>
    /// Provider universal ClusterClient transport which internally uses different runtime-dependent implementations.
    /// </summary>
    public class UniversalTransport : ITransport
    {
        private readonly UniversalTransportSettings settings;
        private readonly ILog log;
        private readonly object sync = new object();
        private ITransport implementation;

        /// <inheritdoc cref="UniversalTransport" />
        public UniversalTransport(UniversalTransportSettings settings, ILog log)
        {
            this.settings = settings.Clone();
            this.log = log;
        }
        
        /// <inheritdoc cref="UniversalTransport" />
        public UniversalTransport(ILog log)
            : this(new UniversalTransportSettings(), log)
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