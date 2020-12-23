using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Sockets
{
    [TestFixture]
    internal class ErrorHandler_Tests
    {
        private ErrorHandler handler;
        private Request request;
        private CancellationTokenSource cancellation;

        [SetUp]
        public void TestSetup()
        {
            handler = new ErrorHandler(new SynchronousConsoleLog());

            request = Request.Get("http://foo/bar");

            cancellation = new CancellationTokenSource();
        }

        [Test]
        public void Should_not_handle_StreamAlreadyUsedException()
        {
            Handle(new StreamAlreadyUsedException("")).Should().BeNull();
        }

        [Test]
        public void Should_return_stream_input_failure_response_for_UserStreamException()
        {
            Handle(new UserStreamException("", null)).Code.Should().Be(ResponseCode.StreamInputFailure);
        }

        [Test]
        public void Should_return_send_failure_response_for_BodySendException()
        {
            Handle(new BodySendException("", null)).Code.Should().Be(ResponseCode.SendFailure);
        }

        [Test]
        public void Should_return_canceled_response_for_any_exception_if_token_is_signaled()
        {
            cancellation.Cancel();

            Handle(new HttpRequestException()).Code.Should().Be(ResponseCode.Canceled);
            Handle(new TaskCanceledException()).Code.Should().Be(ResponseCode.Canceled);
        }

        [Test]
        public void Should_return_unknown_failure_response_for_arbitrary_HttpRequestException()
        {
            Handle(new HttpRequestException()).Code.Should().Be(ResponseCode.UnknownFailure);
        }

        [Test]
        public void Should_return_unknown_failure_response_for_arbitrary_Exception()
        {
            Handle(new Exception()).Code.Should().Be(ResponseCode.UnknownFailure);
        }

        [Test]
        public void Should_return_connection_failure_response_for_inner_TaskCanceledException_when_token_is_not_signaled()
        {
            Handle(new HttpRequestException("", new TaskCanceledException())).Code.Should().Be(ResponseCode.ConnectFailure);
        }

        [Test]
        public void Should_return_connection_failure_response_for_TaskCanceledException_when_token_is_not_signaled()
        {
            Handle(new TaskCanceledException()).Code.Should().Be(ResponseCode.ConnectFailure);
        }

        [TestCase(SocketError.HostDown)]
        [TestCase(SocketError.HostNotFound)]
        [TestCase(SocketError.HostUnreachable)]
        [TestCase(SocketError.NetworkDown)]
        [TestCase(SocketError.NetworkUnreachable)]
        [TestCase(SocketError.AddressNotAvailable)]
        [TestCase(SocketError.AddressAlreadyInUse)]
        [TestCase(SocketError.ConnectionRefused)]
        [TestCase(SocketError.ConnectionAborted)]
        [TestCase(SocketError.ConnectionReset)]
        [TestCase(SocketError.TimedOut)]
        [TestCase(SocketError.TryAgain)]
        [TestCase(SocketError.SystemNotReady)]
        [TestCase(SocketError.TooManyOpenSockets)]
        [TestCase(SocketError.NoBufferSpaceAvailable)]
        [TestCase(SocketError.DestinationAddressRequired)]
        public void Should_return_connection_failure_response_for_inner_SocketException_with_given_code(SocketError code)
        {
            Handle(new HttpRequestException("", new SocketException((int)code))).Code.Should().Be(ResponseCode.ConnectFailure);

            Handle(new HttpRequestException("", new IOException("", new SocketException((int)code)))).Code.Should().Be(ResponseCode.ConnectFailure);
        }

        [Test]
        public void Should_return_receive_failure_response_for_IOException_without_inner_exceptions()
        {
            Handle(new HttpRequestException("", new IOException())).Code.Should().Be(ResponseCode.ReceiveFailure);
        }

        [Test]
        public void Should_return_unknown_failure_response_for_IOException_with_inner_exception()
        {
            Handle(new HttpRequestException("", new IOException("", new Exception()))).Code.Should().Be(ResponseCode.UnknownFailure);
        }

        private Response Handle(Exception error)
            => handler.TryHandle(request, error, cancellation.Token, null);
    }
}
