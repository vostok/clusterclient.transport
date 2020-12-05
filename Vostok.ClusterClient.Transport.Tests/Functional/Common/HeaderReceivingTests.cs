using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Commons.Environment;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class HeaderReceivingTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [TestCase(HeaderNames.ContentEncoding, "identity")]
        [TestCase(HeaderNames.ContentRange, "bytes 200-1000/67589")]
        [TestCase(HeaderNames.ContentType, "text/html; charset=utf-8")]
        [TestCase(HeaderNames.ETag, "\"bfc13a64729c4290ef5b2c2730249c88ca92d82d\"")]
        [TestCase(HeaderNames.Host, "vm-service")]
        [TestCase(HeaderNames.LastModified, "Wed, 21 Oct 2015 07:28:00 GMT")]
        [TestCase(HeaderNames.Location, "http://server:545/file")]
        [TestCase(HeaderNames.ApplicationIdentity, "Abonents.Service")]
        [TestCase(HeaderNames.RequestPriority, "Sheddable")]
        [TestCase(HeaderNames.RequestTimeout, "345345345")]
        [TestCase(HeaderNames.ApplicationIdentity, "first,second,third")]
        public void Should_correctly_receive_given_header_from_server(string headerName, string headerValue)
        {
            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Headers.Set(headerName, headerValue);
                }))
            {
                var response = Send(Request.Post(server.Url));

                response.Headers[headerName].Should().Be(headerValue);
            }
        }

        [Test]
        public void Should_correctly_receive_a_non_ascii_header_from_server()
        {
            if (!RuntimeDetector.IsDotNet50AndNewer)
                return;

            var responseBuilder = new StringBuilder();

            responseBuilder.AppendLine("HTTP/1.1 200 OK");
            responseBuilder.AppendLine("Connection: close");
            responseBuilder.AppendLine("Content-Length: 0");
            responseBuilder.AppendLine("Custom-Header: кириллица");
            responseBuilder.AppendLine("Content-Disposition: ещё кириллица");
            responseBuilder.AppendLine();

            using (var server = SocketTestServer.StartNew(responseBuilder.ToString()))
            {
                var response = Send(Request.Post(server.Url));

                response.Headers["Custom-Header"].Should().Be("кириллица");
                response.Headers["Content-Disposition"].Should().Be("ещё кириллица");
            }
        }

        [Test]
        public void Should_be_able_to_receive_content_length_header_without_body_in_response_to_a_HEAD_request()
        {
            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = 123;
                }))
            {
                var response = Send(Request.Head(server.Url));

                response.Headers["Content-Length"].Should().Be("123");
            }
        }
    }
}
