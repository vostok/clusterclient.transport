using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class BufferingStreamWrapper_Tests
    {
        private BufferingStreamWrapper wrapper;
        private Stream stream;
        private byte[] buffer;

        [SetUp]
        public void SetUp()
        {
            stream = Substitute.For<Stream>();
            wrapper = new BufferingStreamWrapper(stream);
            buffer = new byte[1];
        }

        [Test]
        public void WriteAsync_should_delegate_to_base_stream_as_is_when_buffer_less_than_LOH_objects_size()
        {
            var token = new CancellationTokenSource().Token;
            buffer = ThreadSafeRandom.NextBytes(40 * 1024);

            wrapper.WriteAsync(buffer, 0, buffer.Length, token).GetAwaiter().GetResult();

            stream.Received(1).WriteAsync(buffer, 0, buffer.Length, token);
        }

        [Test]
        public void WriteAsync_should_delegate_to_base_stream_by_small_buffers_when_buffer_greater_than_LOH_objects_size()
        {
            var token = new CancellationTokenSource().Token;
            buffer = ThreadSafeRandom.NextBytes(100 * 1024);

            wrapper.WriteAsync(buffer, 0, buffer.Length, token).GetAwaiter().GetResult();

            stream.Received(6).WriteAsync(Arg.Any<byte[]>(), 0, 16 * 1024, token);
            stream.Received(1).WriteAsync(Arg.Any<byte[]>(), 0, 4 * 1024, token);
        }

        [Test]
        public void WriteAsync_should_wrap_exceptions_in_user_buffering_stream_wrapper_exceptions()
        {
            var error = new Exception("I failed!");

            stream.WriteAsync(default, default, default, default).ThrowsForAnyArgs(error);

            Action action = () => wrapper.WriteAsync(buffer, 0, 1).GetAwaiter().GetResult();

            action.Should().Throw<BufferingStreamWrapperException>().Which.InnerException.Should().BeSameAs(error);
        }
    }
}