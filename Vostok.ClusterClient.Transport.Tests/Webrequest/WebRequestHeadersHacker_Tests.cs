using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Clusterclient.Transport.Webrequest;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Webrequest
{
    [TestFixture]
    internal class WebRequestHeadersHacker_Tests : RuntimeSpecificFixture
    {
        private ILog log;

        [SetUp]
        public void TestSetup()
            => log = new SynchronousConsoleLog();

        [Test]
        public void Should_allow_to_set_values_directly_for_restricted_headers()
        {
            var request = WebRequest.CreateHttp("http://kontur.ru/");

            WebRequestHeadersHacker.TryUnlockRestrictedHeaders(request, log).Should().BeTrue();

            Action unsafeAssignment = () =>
            {
                request.Headers["Accept"] = "123";
                request.Headers["Range"] = "456";
                request.Headers["Content-Length"] = "789";
            };

            unsafeAssignment.Should().NotThrow();

            request.Headers["Accept"].Should().Be("123");
            request.Headers["Range"].Should().Be("456");
            request.Headers["Content-Length"].Should().Be("789");
        }

        protected override Runtime SupportedRuntimes => Runtime.Framework | Runtime.Mono;
    }
}
