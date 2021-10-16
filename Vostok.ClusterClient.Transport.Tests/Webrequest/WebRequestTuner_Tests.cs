using System.Net;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Clusterclient.Transport.Webrequest;
using Vostok.Commons.Environment;

#if NETFRAMEWORK
using System;
using FluentAssertions;
using Vostok.Logging.Abstractions;
#endif

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Clusterclient.Transport.Tests.Webrequest
{
    [TestFixture]
    internal class WebRequestTuner_Tests : RuntimeSpecificFixture
    {
        protected override Runtime SupportedRuntimes => Runtime.Framework | Runtime.Mono;

        [Test]
        public void Should_successfully_tune_http_web_request()
        {
            1.Should().Be(2);
            
            var request = WebRequest.CreateHttp("http://kontur.ru/");

            WebRequestTuner.Tune(request, 1.Seconds(), new WebRequestTransportSettings());

            request.GetHashCode();
        }

        [Test]
        public void Woohoo()
        {
            if (!RuntimeDetector.IsDotNetFramework)
                Assert.Fail();
        }

#if NETFRAMEWORK
        [Test]
        public void Should_tune_first_web_request_correctly()
        {
            var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            try
            {
                var instance = (SeparateAppDomainTest)domain.CreateInstanceFromAndUnwrap(
                    typeof(SeparateAppDomainTest).Assembly.Location,
                    typeof(SeparateAppDomainTest).FullName);

                instance.Test();
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        private class SeparateAppDomainTest : MarshalByRefObject
        {
            public void Test()
            {
                var settings = new WebRequestTransportSettings();

                new WebRequestTransport(settings, new SilentLog()).GetHashCode();

                HttpWebRequest.DefaultMaximumErrorResponseLength.Should().Be(-1);

                var request = WebRequest.CreateHttp("http://kontur.ru/");

                WebRequestTuner.Tune(request, 1.Seconds(), new WebRequestTransportSettings());

                request.MaximumResponseHeadersLength.Should().Be(int.MaxValue);
            }
        }
#endif
    }
}
