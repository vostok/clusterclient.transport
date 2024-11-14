using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Header;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class RequestHeadersConverter_Tests : RuntimeSpecificFixture
    {
        private HttpRequestMessage message;

        [SetUp]
        public void TestSetup()
        {
            message = new HttpRequestMessage();
        }

        [Test]
        public void Should_be_able_to_convert_all_standard_headers()
        {
            foreach (var header in typeof(HeaderNames)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(field => field.GetValue(null) as string)
                .Where(name => name != null)
                .Except(new[]
                {
                    HeaderNames.Connection,
                    HeaderNames.ContentLength,
                    HeaderNames.Host,
                    HeaderNames.TransferEncoding,
                }))
            {
                var value = Guid.NewGuid().ToString();

                var request = Request.Get("/")
                    .WithHeader(header, value);

                RequestHeadersConverter.Fill(request, message, new SynchronousConsoleLog());

                message.Headers.TryGetValues(header, out var observedValue).Should().BeTrue($"Header = '{header}'");

                observedValue.Should().ContainSingle().Which.Should().Be(value);
            }
        }

        [TestCase("\n")]
        [TestCase("\r")]
        [TestCase("\n\r")]
        [TestCase("\r\n")]
        public void Should_throw_error_for_headers_with_new_line(string value)
        {
            var request = Request.Get("/")
                .WithHeader("test", value);
            
            var action = () => RequestHeadersConverter.Fill(request, message, new SynchronousConsoleLog());

            action.Should().Throw<InvalidOperationException>();
        }

        protected override Runtime SupportedRuntimes => Runtime.Core20 | Runtime.Core21 | Runtime.Core31 | Runtime.Core50;
    }
}
