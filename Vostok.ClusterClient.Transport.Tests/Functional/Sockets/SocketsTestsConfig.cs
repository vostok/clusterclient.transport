using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Sockets
{
    internal class SocketsTestsConfig : ITransportTestConfig
    {
        public ITransport CreateTransport(UniversalTransportSettings settings, ILog log) 
            => new SocketsTransport(settings.ToSocketsTransportSettings(), log);

        public Runtime Runtimes => Runtime.Core21 | Runtime.Core31;
    }
}
