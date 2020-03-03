using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal interface ITransportTestConfig
    {
        ITransport CreateTransport(UniversalTransportSettings settings, ILog log);

        Runtime Runtimes { get; }
    }
}
