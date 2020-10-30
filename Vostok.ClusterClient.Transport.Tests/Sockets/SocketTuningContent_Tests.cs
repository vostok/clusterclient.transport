using System.Net.Http.Headers;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Transport.Tests.Sockets
{
    [TestFixture]
    internal class SocketTuningContent_Tests
    {
        [Test]
        public void Should_copy_content_headers()
        {
            var content = new BufferContent(new Content(new byte[16]), default);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var tuningContent = new SocketTuningContent(content, null, new SynchronousConsoleLog());

            tuningContent.Headers.ContentLength.Should().Be(16);

            tuningContent.Headers.ContentType.Should().Be(content.Headers.ContentType);

            tuningContent.Headers.Should().BeSameAs(content.Headers);
        }
    }
}
