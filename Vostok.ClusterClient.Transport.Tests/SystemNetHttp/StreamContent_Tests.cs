using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using StreamContent = Vostok.Clusterclient.Transport.SystemNetHttp.Contents.StreamContent;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class StreamContent_Tests
    {
        private Core.Model.IStreamContent content;
        private Stream stream;
        private long length;
        private StreamContent streamContent;

        [SetUp]
        public void TestSetup()
        {
            content = Substitute.For<Core.Model.IStreamContent>();
            content.Stream.Returns(_ => stream);
            content.Length.Returns(_ => length);

            stream = new MemoryStream(Guid.NewGuid().ToByteArray());
            length = stream.Length;

            streamContent = new StreamContent(content, CancellationToken.None);
        }

        [Test]
        public void Should_have_correct_content_length_header()
        {
            streamContent.Headers.ContentLength.Should().Be(length);
        }

        [Test]
        public void Should_return_correct_content_when_buffered()
        {
            streamContent.ReadAsByteArrayAsync().Result.Should().Equal(((MemoryStream)stream).ToArray());
        }

        [Test]
        public void Should_not_swallow_cancellation_errors()
        {
            stream = Substitute.For<Stream>();

            stream.ReadAsync(default, default, default, default).ThrowsForAnyArgs(new TaskCanceledException());

            Action action = () => streamContent.CopyToAsync(Stream.Null).GetAwaiter().GetResult();

            action.Should().Throw<TaskCanceledException>();
        }

        [Test]
        public void Should_not_swallow_user_stream_errors()
        {
            var streamError = new Exception("I failed!");

            stream = Substitute.For<Stream>();

            stream.ReadAsync(default, default, default, default).ThrowsForAnyArgs(streamError);

            Action action = () => streamContent.CopyToAsync(Stream.Null).GetAwaiter().GetResult();

            action.Should().Throw<UserStreamException>().Which.InnerException.Should().BeSameAs(streamError);
        }

        [Test]
        public void Should_wrap_generic_errors_into_body_send_exceptions()
        {
            var streamError = new Exception("I failed!");

            var targetStream = Substitute.For<Stream>();

            targetStream.WriteAsync(default, default, default, default).ThrowsForAnyArgs(streamError);

            Action action = () => streamContent.CopyToAsync(targetStream).GetAwaiter().GetResult();

            action.Should().Throw<BodySendException>().Which.InnerException.Should().BeSameAs(streamError);
        }
    }
}
