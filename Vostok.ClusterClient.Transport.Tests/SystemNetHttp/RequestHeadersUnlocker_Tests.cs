using System.Net.Http;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.SystemNetHttp.Header;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class RequestHeadersUnlocker_Tests : RuntimeSpecificFixture
    {
        [Test]
        public void Should_successfully_unlock_headers_of_HttpRequestMessage()
        {
            var message = new HttpRequestMessage();

            RequestHeadersUnlocker.TryUnlockRestrictedHeaders(message.Headers, new SynchronousConsoleLog()).Should().BeTrue();
        }

        protected override Runtime SupportedRuntimes => Runtime.Core20 | Runtime.Core21 | Runtime.Core31; 
    }
}
