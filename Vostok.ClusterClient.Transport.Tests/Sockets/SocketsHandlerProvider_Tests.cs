using System;
using System.Net;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Sockets
{
    [TestFixture]
    internal class SocketsHandlerProvider_Tests : RuntimeSpecificFixture
    {
        private SocketsTransportSettings settings;
        private SocketsHandlerProvider provider;

        [SetUp]
        public void TestSetup()
        {
            settings = new SocketsTransportSettings();
            provider = new SocketsHandlerProvider(settings);
        }

        [Test]
        public void Should_return_a_handler_with_proxy_from_settings()
        {
            var proxy = Substitute.For<IWebProxy>();

            settings.Proxy = proxy;

            ObtainAndGetProperty<IWebProxy>(null, "Proxy").Should().BeSameAs(proxy);
        }

        [Test]
        public void Should_return_a_handler_with_given_connection_timeout()
        {
            ObtainAndGetProperty<TimeSpan>(1.Seconds(), "ConnectTimeout").Should().Be(1.Seconds());
            ObtainAndGetProperty<TimeSpan>(2.Seconds(), "ConnectTimeout").Should().Be(2.Seconds());
            ObtainAndGetProperty<TimeSpan>(3.Seconds(), "ConnectTimeout").Should().Be(3.Seconds());
            ObtainAndGetProperty<TimeSpan>(null, "ConnectTimeout").Should().Be(Timeout.InfiniteTimeSpan);
        }

        [Test]
        public void Should_return_a_handler_with_given_connection_idle_timeout()
        {
            settings.ConnectionIdleTimeout = 5.Seconds();

            ObtainAndGetProperty<TimeSpan>(1.Seconds(), "PooledConnectionIdleTimeout").Should().Be(5.Seconds());
        }

        [Test]
        public void Should_return_a_handler_with_given_connection_lifetime()
        {
            settings.ConnectionLifetime = 10.Seconds();

            ObtainAndGetProperty<TimeSpan>(1.Seconds(), "PooledConnectionLifetime").Should().Be(10.Seconds());
        }

        [Test]
        public void Should_return_a_handler_with_given_max_connections_per_endpoint()
        {
            settings.MaxConnectionsPerEndpoint = 123;

            ObtainAndGetProperty<int>(1.Seconds(), "MaxConnectionsPerServer").Should().Be(123);
        }

        [Test]
        public void Should_return_a_handler_with_given_auto_redirect_flag()
        {
            settings.AllowAutoRedirect = false;

            ObtainAndGetProperty<bool>(1.Seconds(), "AllowAutoRedirect").Should().BeFalse();
        }

        [Test]
        public void Should_cache_handler_in_one_instance()
        {
            var handler1 = provider.Obtain(null);
            var handler2 = provider.Obtain(null);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_cache_handler_between_multiple_instances()
        {
            var handler1 = provider.Obtain(null);
            var handler2 = new SocketsHandlerProvider(settings).Obtain(null);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_return_different_handlers_for_different_connection_timeouts()
        {
            var handler1 = provider.Obtain(null);
            var handler2 = provider.Obtain(1.Seconds());

            handler2.Should().NotBeSameAs(handler1);
        }

        [Test]
        public void Should_return_different_handlers_for_different_settings()
        {
            var settings2 = new SocketsTransportSettings {ConnectionLifetime = 5.Hours()};
            var provider2 = new SocketsHandlerProvider(settings2);

            var handler1 = provider.Obtain(null);
            var handler2 = provider2.Obtain(null);

            handler2.Should().NotBeSameAs(handler1);
        }

        protected override Runtime SupportedRuntimes => Runtime.Core21 | Runtime.Core31;

        private T ObtainAndGetProperty<T>(TimeSpan? connectionTimeout, string propertyName)
        {
            var handler = provider.Obtain(connectionTimeout);

            return (T)handler?.GetType().GetProperty(propertyName)?.GetValue(handler);
        }
    }
}
