using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Native;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Native
{
    [TestFixture]
    internal class HttpClientProvider_Tests : RuntimeSpecificFixture
    {
        private HttpClientProvider provider;
        private NativeTransportSettings settings;

        private ILog log;

        private readonly TimeSpan connectionTimeout = 10.Seconds();

        [SetUp]
        public void TestSetup()
        {
            log = new ConsoleLog();

            settings = new NativeTransportSettings();
            provider = new HttpClientProvider(settings, log);
        }

        [Test]
        public void Should_cache_handler_in_one_instance()
        {
            var handler1 = provider.Obtain(connectionTimeout);
            var handler2 = provider.Obtain(connectionTimeout);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_cache_handler_between_multiple_instances()
        {
            var handler1 = provider.Obtain(connectionTimeout);
            var handler2 = new HttpClientProvider(settings, log).Obtain(connectionTimeout);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_cache_handler_by_equal_settings()
        {
            var universalTransportSettings = new UniversalTransportSettings();
            var settings1 = universalTransportSettings.ToNativeTransportSettings();
            var settings2 = universalTransportSettings.ToNativeTransportSettings();

            var handler1 = new HttpClientProvider(settings1, log).Obtain(connectionTimeout);
            var handler2 = new HttpClientProvider(settings2, log).Obtain(connectionTimeout);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_return_different_handlers_for_different_certificate_validation_callback()
        {
            var universalTransportSettings = new UniversalTransportSettings();
            var settings1 = universalTransportSettings.ToNativeTransportSettings();
            var settings2 = universalTransportSettings.ToNativeTransportSettings();

            settings1.RemoteCertificateValidationCallback = (message, certificate2, arg3, arg4) => true;

            var handler1 = new HttpClientProvider(settings1, log).Obtain(connectionTimeout);
            var handler2 = new HttpClientProvider(settings2, log).Obtain(connectionTimeout);

            handler2.Should().NotBeSameAs(handler1);
        }

        [Test]
        public void Should_cache_handler_with_equal_credentials()
        {
            var universalTransportSettings = new UniversalTransportSettings();
            var settings1 = universalTransportSettings.ToNativeTransportSettings();
            var settings2 = universalTransportSettings.ToNativeTransportSettings();

            settings1.Credentials = new NetworkCredential("u1", "p1");
            settings2.Credentials = new NetworkCredential("u1", "p1");

            var handler1 = new HttpClientProvider(settings1, log).Obtain(connectionTimeout);
            var handler2 = new HttpClientProvider(settings2, log).Obtain(connectionTimeout);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_cache_handler_with_equal_credentials2()
        {
            var universalTransportSettings = new UniversalTransportSettings();
            var settings1 = universalTransportSettings.ToNativeTransportSettings();
            var settings2 = universalTransportSettings.ToNativeTransportSettings();

            var credentials = new NetworkCredential("u1", "p1");

            settings1.Credentials = credentials;
            settings2.Credentials = credentials;

            var handler1 = new HttpClientProvider(settings1, log).Obtain(connectionTimeout);
            var handler2 = new HttpClientProvider(settings2, log).Obtain(connectionTimeout);

            handler2.Should().BeSameAs(handler1);
        }

        [Test]
        public void Should_return_different_handlers_for_different_credentials()
        {
            var universalTransportSettings = new UniversalTransportSettings();
            var settings1 = universalTransportSettings.ToNativeTransportSettings();
            var settings2 = universalTransportSettings.ToNativeTransportSettings();

            settings1.Credentials = new NetworkCredential("u1", "p1");
            settings2.Credentials = new NetworkCredential("u1", "p2");

            var handler1 = new HttpClientProvider(settings1, log).Obtain(connectionTimeout);
            var handler2 = new HttpClientProvider(settings2, log).Obtain(connectionTimeout);

            handler2.Should().NotBeSameAs(handler1);
        }

        protected override Runtime SupportedRuntimes => Runtime.Core20;
    }
}