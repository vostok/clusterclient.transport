using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Clusterclient.Transport.SystemNetHttp.Messages;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Console;
using StreamContent = Vostok.Clusterclient.Transport.SystemNetHttp.Contents.StreamContent;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class RequestMessageFactory_Tests : RuntimeSpecificFixture
    {
        private Request request;
        private HttpRequestMessage message;

        [SetUp]
        public void TestSetup()
            => request = Request.Get("api/v1/user/alesha?key=value");

        [Test]
        public void Should_preserve_request_url_as_is()
        {
            Convert();

            message.RequestUri.Should().BeSameAs(request.Url);
        }

        [Test]
        public void Should_translate_all_request_methods()
        {
            foreach (var method in typeof(RequestMethods)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field => field.GetValue(null))
                .Cast<string>())
            {
                request = new Request(method, request.Url);

                Convert();

                message.Method.Method.Should().Be(method);
            }
        }

        [Test]
        public void Should_create_a_message_with_empty_content_if_request_has_no_body()
        {
            Convert();

            message.Content.Should().BeOfType<EmptyContent>();
        }

        [Test]
        public void Should_create_a_message_with_empty_content_if_request_has_an_empty_body()
        {
            request = request.WithContent(new byte[0]);

            Convert();

            message.Content.Should().BeOfType<EmptyContent>();
        }

        [Test]
        public void Should_create_a_message_with_buffer_content_if_request_has_an_in_memory_body()
        {
            request = request.WithContent(new byte[1]);

            Convert();

            message.Content.Should().BeOfType<BufferContent>();
        }

        [Test]
        public void Should_create_a_message_with_stream_content_if_request_has_a_stream_as_body()
        {
            request = request.WithContent(new MemoryStream());

            Convert();

            message.Content.Should().BeOfType<StreamContent>();
        }

        [Test]
        public void Should_create_a_message_with_composite_content_if_request_has_a_composite_buffer_body()
        {
            request = request.WithContent(new[] { new byte[1], new byte[2] });

            Convert();

            message.Content.Should().BeOfType<CompositeBufferContent>();
        }

        [Test]
        public void Should_create_a_message_with_content_producer_if_request_has_a_content_producer_body()
        {
            var contentProducer = ContentProducerFactory.BuildRandomStreamContentProducer(2, length: 2);

            request = request.WithContent(contentProducer);

            Convert();

            message.Content.Should().BeOfType<ContentProducerContent>();
        }

        [Test]
        public void Should_fill_message_with_request_headers()
        {
            request = request
                .WithHeader("key1", "value1")
                .WithHeader("key2", "value2");

            Convert();

            message.Headers.GetValues("key1").Should().ContainSingle().Which.Should().Be("value1");
            message.Headers.GetValues("key2").Should().ContainSingle().Which.Should().Be("value2");
        }

        private void Convert()
            => message = RequestMessageFactory.Create(request, CancellationToken.None, new SynchronousConsoleLog());

        protected override Runtime SupportedRuntimes => Runtime.Core20 | Runtime.Core21 | Runtime.Core31;
    }
}
