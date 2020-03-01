using System;
using System.IO;
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
    internal class UserStreamWrapper_Tests
    {
        private Stream stream;
        private UserStreamWrapper wrapper;
        private byte[] buffer;

        [SetUp]
        public void TestSetup()
        {
            stream = Substitute.For<Stream>();
            wrapper = new UserStreamWrapper(stream);
            buffer = new byte[1];
        }

        [Test]
        public void ReadAsync_should_delegate_to_base_stream()
        {
            var token = new CancellationTokenSource().Token;

            wrapper.ReadAsync(buffer, 0, 1, token).GetAwaiter().GetResult();

            stream.Received(1).ReadAsync(buffer, 0, 1, token);
        }

        [Test]
        public void ReadAsync_should_pass_cancellation_exceptions_as_is()
        {
            stream.ReadAsync(default, default, default, default).ThrowsForAnyArgs(new TaskCanceledException());

            Action action = () => wrapper.ReadAsync(buffer, 0, 1).GetAwaiter().GetResult();

            action.Should().Throw<TaskCanceledException>();
        }

        [Test]
        public void ReadAsync_should_pass_reuse_exceptions_as_is()
        {
            stream.ReadAsync(default, default, default, default).ThrowsForAnyArgs(new StreamAlreadyUsedException(""));

            Action action = () => wrapper.ReadAsync(buffer, 0, 1).GetAwaiter().GetResult();

            action.Should().Throw<StreamAlreadyUsedException>();
        }

        [Test]
        public void ReadAsync_should_wrap_exceptions_in_user_stream_exceptions()
        {
            var error = new Exception("I failed!");

            stream.ReadAsync(default, default, default, default).ThrowsForAnyArgs(error);

            Action action = () => wrapper.ReadAsync(buffer, 0, 1).GetAwaiter().GetResult();

            action.Should().Throw<UserStreamException>().Which.InnerException.Should().BeSameAs(error);
        }
    }
}
