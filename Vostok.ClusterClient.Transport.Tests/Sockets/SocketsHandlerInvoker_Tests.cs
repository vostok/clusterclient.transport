using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Sockets;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Sockets
{
    [TestFixture]
    internal class SocketsHandlerInvoker_Tests : RuntimeSpecificFixture
    {
        [Test]
        public void Should_be_able_to_invoke_SocketsHttpHandler_directly()
            => SocketsHandlerInvoker.CanInvokeDirectly.Should().BeTrue();

        protected override Runtime SupportedRuntimes => Runtime.Core21 | Runtime.Core31;
    }
}
