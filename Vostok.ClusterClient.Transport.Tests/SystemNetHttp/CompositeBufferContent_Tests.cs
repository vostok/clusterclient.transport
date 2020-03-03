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
    internal class CompositeBufferContent_Tests
    {
        private byte[] usefulData1;
        private byte[] usefulData2;
        private byte[] usefulData3;
        private byte[] totalUsefulData;
        private byte[] buffer1;
        private byte[] buffer2;
        private byte[] buffer3;
        private CancellationTokenSource cancellation;
        private CompositeBufferContent content;

        [SetUp]
        public void TestSetup()
        {
            usefulData1 = Guid.NewGuid().ToByteArray();
            usefulData2 = Guid.NewGuid().ToByteArray();
            usefulData3 = Guid.NewGuid().ToByteArray();

            totalUsefulData = usefulData1.Concat(usefulData2).Concat(usefulData3).ToArray();

            buffer1 = new byte[3].Concat(usefulData1).Concat(new byte[6]).ToArray();
            buffer2 = new byte[4].Concat(usefulData2).Concat(new byte[6]).ToArray();
            buffer3 = new byte[5].Concat(usefulData3).Concat(new byte[6]).ToArray();

            var part1 = new Content(buffer1, 3, usefulData1.Length);
            var part2 = new Content(buffer2, 4, usefulData2.Length);
            var part3 = new Content(buffer3, 5, usefulData3.Length);

            cancellation = new CancellationTokenSource();

            content = new CompositeBufferContent(new CompositeContent(new[] { part1, part2, part3 }), cancellation.Token);
        }

        [Test]
        public void Should_have_correct_content_length_header()
        {
            content.Headers.ContentLength.Should().Be(totalUsefulData.Length);
        }

        [Test]
        public void Should_be_read_as_correct_portions_of_small_buffers()
        {
            content.ReadAsByteArrayAsync().Result.Should().Equal(totalUsefulData);
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
