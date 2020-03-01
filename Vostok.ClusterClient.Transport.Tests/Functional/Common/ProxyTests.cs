using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class ProxyTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public virtual void Should_send_request_to_proxy_if_setting_is_true()
        {
            using (var proxy = TestServer.StartNew(ctx => ctx.Response.StatusCode = 201))
            {
                settings.Proxy = new WebProxy(proxy.Url, false);

                var expectedUrl = new Uri($"http://vostok:{proxy.Port}");
                var request = Request.Get("http://vostok");
                var response = Send(request);
                
                response.Code.Should().Be(ResponseCode.Created);
                proxy.LastRequest.Url.Should().Be(expectedUrl);
            }
        }
    }
}
