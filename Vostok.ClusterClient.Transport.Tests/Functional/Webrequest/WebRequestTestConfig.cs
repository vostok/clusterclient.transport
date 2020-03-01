using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;
using Vostok.Clusterclient.Transport.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Webrequest
{
    internal class WebRequestTestConfig : ITransportTestConfig
    {
        public ITransport CreateTransport(UniversalTransportSettings settings, ILog log) 
            => new WebRequestTransport(settings.ToWebRequestTransportSettings(), log);

        public Runtime Runtimes => Runtime.Framework | Runtime.Mono;
    }
}
