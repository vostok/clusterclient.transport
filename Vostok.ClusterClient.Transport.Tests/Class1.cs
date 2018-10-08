using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;
using Vostok.ClusterClient.Transport.Tests.Functional;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.ClusterClient.Transport.Tests
{
    public class Config : ITransportTestConfig
    {
        public ILog CreateLog() => new ConsoleLog();

        public ITransport CreateTransport(TestTransportSettings settings, ILog log)
        {
            var transportSettings = new UniversalTransportSettings
            {
                UseResponseStreaming = settings.UseResponseStreaming,
                ConnectionAttempts = settings.ConnectionAttempts,
                ConnectionTimeout = settings.ConnectionTimeout,
                AllowAutoRedirect = settings.AllowAutoRedirect,
                MaxResponseBodySize = settings.MaxResponseBodySize
            };
            return new UniversalTransport(transportSettings, log);
        }

        public TestTransportSettings CreateDefaultSettings() => new TestTransportSettings
        {
            MaxConnectionsPerEndpoint = 10 * 1000,
            ConnectionAttempts = 2,
            ConnectionTimeout = TimeSpan.FromMilliseconds(750),
            BufferFactory = size => new byte[size],
            UseResponseStreaming = _ => false
        };
    }

    [TestFixture]
    internal class AllowAutoRedirectTests : AllowAutoRedirectTests<Config>
    {
    }
    
    internal class ClientTimeoutTests : ClientTimeoutTests<Config>
    {
    }
    
    internal class ConnectionFailureTests : ConnectionFailureTests<Config>
    {
    }
    internal class ConnectionTimeoutTests : ConnectionTimeoutTests<Config>
    {
    }
    internal class ContentReceivingTests : ContentReceivingTests<Config>
    {
    }
    internal class ContentSendingTests : ContentSendingTests<Config>
    {
    }
    internal class HeaderReceivingTests : HeaderReceivingTests<Config>
    {
    }
    internal class HeaderSendingTests : HeaderSendingTests<Config>
    {
    }
    internal class MaxConnectionsPerEndpointTests : MaxConnectionsPerEndpointTests<Config>
    {
    }
    internal class MethodSendingTests : MethodSendingTests<Config>
    {
    }
    internal class ProxyTests : ProxyTests<Config>
    {
    }
    internal class QuerySendingTests : QuerySendingTests<Config>
    {
    }
    internal class RequestCancellationTests : RequestCancellationTests<Config>
    {
    }
    internal class StatusCodeReceivingTests : StatusCodeReceivingTests<Config>
    {
    }
}