using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class BufferContent_Tests
    {
        private byte[] usefulData;
        private byte[] totalBuffer;
        private CancellationTokenSource cancellation;
        private BufferContent content;

        [SetUp]
        public void TestSetup()
        {
            usefulData = Guid.NewGuid().ToByteArray();

            totalBuffer = Guid.NewGuid()
                .ToByteArray()
                .Concat(usefulData)
                .Concat(Guid.NewGuid().ToByteArray())
                .ToArray();

            cancellation = new CancellationTokenSource();

            content = new BufferContent(new Content(totalBuffer, usefulData.Length, usefulData.Length), cancellation.Token);
        }

        [Test]
        public void Should_have_correct_content_length_header()
        {
            content.Headers.ContentLength.Should().Be(usefulData.Length);
        }

        [Test]
        public void Should_be_read_as_correct_portion_of_small_buffer()
        {
            content.ReadAsByteArrayAsync().Result.Should().Equal(usefulData);
        }

        [Test]
        public void Should_be_read_as_correct_portion_of_large_buffer()
        {
            usefulData = new byte[100 * 1000];

            new Random(Guid.NewGuid().GetHashCode()).NextBytes(usefulData);

            totalBuffer = Guid.NewGuid()
                .ToByteArray()
                .Concat(usefulData)
                .Concat(Guid.NewGuid().ToByteArray())
                .ToArray();

            content = new BufferContent(new Content(totalBuffer, 16, usefulData.Length), cancellation.Token);

            content.ReadAsByteArrayAsync().Result.Should().Equal(usefulData);
        }

        [Test]
        public void Should_respect_cancellation_token()
        {
            cancellation.Cancel();

            Action action = () => content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

            action.Should().Throw<TaskCanceledException>();
        }

        [Test]
        public void Should_wrap_errors_into_body_send_exceptions()
        {
            var error = new Exception("Fail!");

            var stream = Substitute.For<Stream>();

            stream.WriteAsync(default, default, default, default).ThrowsForAnyArgs(error);

            Action action = () => content.CopyToAsync(stream).GetAwaiter().GetResult();

            action.Should().Throw<BodySendException>().Which.InnerException.Should().BeSameAs(error);
        }
    }
}
