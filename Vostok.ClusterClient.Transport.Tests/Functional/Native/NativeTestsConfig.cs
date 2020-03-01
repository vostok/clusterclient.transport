using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Abstractions;

#pragma warning disable 618

namespace Vostok.Clusterclient.Transport.Tests.Functional.Native
{
    internal class NativeTestsConfig : ITransportTestConfig
    {
        public Runtime Runtimes => Runtime.Core20;

        public ITransport CreateTransport(UniversalTransportSettings settings, ILog log) 
            => new NativeTransport(settings.ToNativeTransportSettings(), log);
    }
}
