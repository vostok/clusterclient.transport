using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Clusterclient.Transport.SystemNetHttp.BodyReading;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class BodyReader_Tests
    {
        private byte[] content;
        private MemoryStream stream;
        private HttpResponseMessage message;
        private CancellationTokenSource cancellation;

        private long? maxBodySize;
        private bool useStreaming;

        private BodyReader reader;

        [SetUp]
        public void TestSetup()
        {
            content = new byte[157381];

            new Random(Guid.NewGuid().GetHashCode()).NextBytes(content);

            stream = new MemoryStream(content, 0, content.Length);

            message = new HttpResponseMessage
            {
                Content = new System.Net.Http.StreamContent(stream)
                {
                    Headers =
                    {
                        ContentLength = content.Length
                    }
                }
            };

            cancellation = new CancellationTokenSource();

            maxBodySize = null;

            useStreaming = false;

            reader = new BodyReader(len => new byte[len], _ => useStreaming, () => maxBodySize, new SynchronousConsoleLog());
        }

        [Test]
        public void Should_immediately_return_an_empty_content_when_content_length_is_zero()
        {
            message.Content.Headers.ContentLength = 0L;

            var result = Read();

            result.Content.Should().BeSameAs(Content.Empty);
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().BeNull();

            stream.Position.Should().Be(0);
        }

        [Test]
        public void Should_immediately_return_an_error_when_content_length_exceeds_int_max_value()
        {
            message.Content.Headers.ContentLength = int.MaxValue + 1L;

            var result = Read();

            result.Content.Should().BeNull();
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().Be(ResponseCode.InsufficientStorage);

            stream.Position.Should().Be(0);
        }

        [Test]
        public void Should_not_return_an_error_when_content_length_exceeds_int_max_value_but_streaming_is_on()
        {
            useStreaming = true;
            message.Content.Headers.ContentLength = int.MaxValue + 1L;

            var result = Read();

            result.Content.Should().BeNull();
            result.Stream.Should().NotBeNull();
            result.ErrorCode.Should().BeNull();

            stream.Position.Should().Be(0);

            var buffer = new byte[content.Length];

            result.Stream.Read(buffer, 0, buffer.Length).Should().Be(buffer.Length);

            buffer.Should().Equal(content);

            stream.Position.Should().Be(content.Length);
        }

        [Test]
        public void Should_immediately_return_an_error_when_content_length_exceeds_configured_limit()
        {
            maxBodySize = content.Length - 1;

            var result = Read();

            result.Content.Should().BeNull();
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().Be(ResponseCode.InsufficientStorage);

            stream.Position.Should().Be(0);
        }

        [Test]
        public void Should_immediately_return_content_stream_if_streaming_is_enabled()
        {
            useStreaming = true;

            var result = Read();

            result.Content.Should().BeNull();
            result.Stream.Should().NotBeNull();
            result.ErrorCode.Should().BeNull();

            stream.Position.Should().Be(0);

            var buffer = new byte[content.Length];

            result.Stream.Read(buffer, 0, buffer.Length).Should().Be(buffer.Length);

            buffer.Should().Equal(content);

            stream.Position.Should().Be(content.Length);
        }

        [Test]
        public void Should_successfully_read_a_small_body_with_known_length()
        {
            const int length = Constants.BufferSize * 2 + 1;

            message.Content.Headers.ContentLength = length;

            var result = Read();

            result.Content.ToArray().Should().Equal(content.Take(length));
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().BeNull();
        }

        [Test]
        public void Should_successfully_read_a_large_body_with_known_length()
        {
            var result = Read();

            result.Content.ToArray().Should().Equal(content);
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().BeNull();
        }

        [Test]
        public void Should_return_an_error_when_body_stream_does_not_contain_amount_of_data_specified_by_content_length()
        {
            message.Content.Headers.ContentLength = content.Length + 1;

            var result = Read();

            result.Content.Should().BeNull();
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().Be(ResponseCode.ReceiveFailure);
        }

        [Test]
        public void Should_successfully_read_a_large_body_with_unknown_length()
        {
            message.Content.Headers.ContentLength = null;

            var result = Read();

            result.Content.ToArray().Should().Equal(content);
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().BeNull();
        }

        [Test]
        public void Should_return_an_error_when_body_stream_with_unknown_length_exceeds_configured_size_limit()
        {
            message.Content.Headers.ContentLength = null;
            maxBodySize = content.Length - 1;

            var result = Read();

            result.Content.Should().BeNull();
            result.Stream.Should().BeNull();
            result.ErrorCode.Should().Be(ResponseCode.InsufficientStorage);
        }

        [Test]
        public void Should_not_swallow_cancellation_exceptions()
        {
            cancellation.Cancel();

            Action action = () => Read();

            action.Should().Throw<TaskCanceledException>();
        }

        private BodyReadResult Read()
            => reader.ReadAsync(message, cancellation.Token).GetAwaiter().GetResult();
    }
}
