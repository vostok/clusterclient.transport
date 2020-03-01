using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class ConnectionTimeoutTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        private readonly Uri dummyServerUrl = new Uri("http://9.0.0.1:10/");

        [Test]
        public virtual void Should_timeout_on_connection_to_a_blackhole_by_connect_timeout()
        {
            var task = SendAsync(Request.Get(dummyServerUrl), connectionTimeout: 250.Milliseconds());

            task.Wait(5.Seconds()).Should().BeTrue();

            task.Result.Code.Should().Be(ResponseCode.ConnectFailure);
        }

        [Test]
        public void Should_timeout_on_connection_to_a_blackhole_by_full_timeout()
        {
            var task = SendAsync(Request.Get(dummyServerUrl), 500.Milliseconds(), connectionTimeout: 1.Seconds());

            task.Wait(5.Seconds()).Should().BeTrue();

            task.Result.Code.Should().Be(ResponseCode.RequestTimeout);
        }

        [Test]
        public void Should_not_timeout_on_connection_when_server_is_just_slow()
        {
            using (var server = TestServer.StartNew(
                ctx =>
                {
                    Thread.Sleep(2.Seconds());
                    ctx.Response.StatusCode = 200;
                }))
            {
                Send(Request.Get(server.Url), connectionTimeout: 250.Milliseconds()).Code.Should().Be(ResponseCode.Ok);
            }
        }

        [Test]
        public void Should_timeout_by_request_timeout_first()
        {
            var task = SendAsync(Request.Get(dummyServerUrl), 500.Milliseconds(), connectionTimeout: 10.Seconds());

            task.Wait(1.Seconds());

            task.IsCompleted.Should().BeTrue();

            task.Result.Code.Should().Be(ResponseCode.RequestTimeout);
        }
    }
}
