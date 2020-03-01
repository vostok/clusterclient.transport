using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class MethodSendingTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [TestCase(RequestMethods.Delete)]
        [TestCase(RequestMethods.Get)]
        [TestCase(RequestMethods.Head)]
        [TestCase(RequestMethods.Options)]
        [TestCase(RequestMethods.Patch)]
        [TestCase(RequestMethods.Post)]
        [TestCase(RequestMethods.Put)]
        [TestCase(RequestMethods.Trace)]
        public void Should_be_able_to_send_requests_with_given_method(string method)
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                Send(new Request(method, server.Url));

                server.LastRequest.Method.Should().Be(method);
            }
        }
    }
}
