using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Helpers;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class TimeoutProvider_Tests
    {
        private Request request;
        private TimeoutProvider provider;
        private CancellationTokenSource cancellation;

        private bool calledSender;
        private Response senderResponse;
        private CancellationToken senderToken;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("http://foo/bar");

            cancellation = new CancellationTokenSource();

            provider = new TimeoutProvider(50.Milliseconds(), new SynchronousConsoleLog());

            calledSender = false;
            senderResponse = Responses.Ok;
            senderToken = default;
        }

        [Test]
        public void Should_immediately_return_canceled_response_when_given_an_already_canceled_token()
        {
            cancellation.Cancel();

            Send(1.Seconds(), 1.Milliseconds()).Code.Should().Be(ResponseCode.Canceled);

            calledSender.Should().BeFalse();
        }

        [Test]
        public void Should_immediately_return_timeout_response_when_given_a_microscopic_timeout()
        {
            Send(1.Ticks(), 1.Milliseconds()).Code.Should().Be(ResponseCode.RequestTimeout);

            calledSender.Should().BeFalse();
        }

        [Test]
        public void Should_return_response_returned_by_sender_when_it_arrives_in_time()
        {
            Send(1.Minutes(), 5.Milliseconds()).Should().BeSameAs(senderResponse);
        }

        [Test]
        public void Should_return_a_timeout_response_when_sender_does_not_complete_in_time()
        {
            Send(1.Milliseconds(), 5.Seconds()).Code.Should().Be(ResponseCode.RequestTimeout);
        }

        [Test]
        public void Should_cancel_request_sending_upon_returning_a_timeout_response()
        {
            Send(1.Milliseconds(), 5.Seconds());

            senderToken.IsCancellationRequested.Should().BeTrue();
        }

        private Response Send(TimeSpan timeout, TimeSpan sendLatency)
        {
            return provider.SendWithTimeoutAsync((_, _, token) => Task.Run(
                        async () =>
                        {
                            calledSender = true;
                            senderToken = token;
                            await Task.Delay(sendLatency, token);
                            return senderResponse;
                        }),
                    request, timeout, timeout, cancellation.Token)
                .GetAwaiter()
                .GetResult();
        }
    }
}
