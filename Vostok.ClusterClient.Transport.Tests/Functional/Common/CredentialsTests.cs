using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal class CredentialsTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_use_credentials()
        {
            var name = "user1234";
            var password = "qwerty";

            settings.Credentials = new NetworkCredential(name, password);

            using (var server = TestServer.StartNew(handle: ctx => ctx.Response.StatusCode = 200, l => l.AuthenticationSchemes = AuthenticationSchemes.Basic))
            {
                var response = Send(Request.Get(server.Url));

                response.Code.Should().Be(ResponseCode.Ok);

                var identity = server.LastRequest.UserIdentity;
                identity.Name.Should().Be(name);
                identity.Password.Should().Be(password);
            }
        }
    }
}