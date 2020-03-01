using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    [TestFixture]
    internal abstract class TransportFunctionalTests<TConfig> : RuntimeSpecificFixture
        where TConfig : ITransportTestConfig, new()
    {
        protected readonly ILog log = new SynchronousConsoleLog();
        protected UniversalTransportSettings settings;
        
        private readonly TConfig config = new TConfig();

        static TransportFunctionalTests()
            => ThreadPool.SetMinThreads(128, 128);

        [SetUp]
        public void BaseSetUp()
            => settings = new UniversalTransportSettings();

        protected override Runtime SupportedRuntimes => config.Runtimes;

        protected Task<Response> SendAsync(Request request, TimeSpan? timeout = null, CancellationToken cancellationToken = default, TimeSpan? connectionTimeout = null)
            => config.CreateTransport(settings, log).SendAsync(request, connectionTimeout, timeout ?? 1.Minutes(), cancellationToken);

        protected Response Send(Request request, TimeSpan? timeout = null, CancellationToken cancellationToken = default, TimeSpan? connectionTimeout = null)
            => config.CreateTransport(settings, log).SendAsync(request, connectionTimeout, timeout ?? 1.Minutes(), cancellationToken).GetAwaiter().GetResult();
    }
}
