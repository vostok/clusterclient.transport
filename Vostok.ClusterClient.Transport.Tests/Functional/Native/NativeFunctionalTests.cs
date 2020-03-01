using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Native
{
    internal class AllowAutoRedirectTests : AllowAutoRedirectTests<NativeTestsConfig>
    {
    }

    internal class ClientTimeoutTests : ClientTimeoutTests<NativeTestsConfig>
    {
    }

    internal class ConnectionFailureTests : ConnectionFailureTests<NativeTestsConfig>
    {
    }

    internal class ConnectionTimeoutTests : ConnectionTimeoutTests<NativeTestsConfig>
    {
        public override void Should_timeout_on_connection_to_a_blackhole_by_connect_timeout()
        {
            while (!Debugger.IsAttached)
            {
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                base.Should_timeout_on_connection_to_a_blackhole_by_connect_timeout();
        }
    }

    internal class ContentReceivingTests : ContentReceivingTests<NativeTestsConfig>
    {
    }

    internal class ContentSendingTests : ContentSendingTests<NativeTestsConfig>
    {
    }

    internal class ContentStreamingTests : ContentStreamingTests<NativeTestsConfig>
    {
    }

    internal class HeaderReceivingTests : HeaderReceivingTests<NativeTestsConfig>
    {
    }

    internal class HeaderSendingTests : HeaderSendingTests<NativeTestsConfig>
    {
    }

    internal class MaxConnectionsPerEndpointTests : MaxConnectionsPerEndpointTests<NativeTestsConfig>
    {
    }

    internal class MethodSendingTests : MethodSendingTests<NativeTestsConfig>
    {
    }

    internal class NetworkErrorsHandlingTests : NetworkErrorsHandlingTests<NativeTestsConfig>
    {
    }

    internal class ProxyTests : ProxyTests<NativeTestsConfig>
    {
    }

    internal class QuerySendingTests : QuerySendingTests<NativeTestsConfig>
    {
    }

    internal class RequestCancellationTests : RequestCancellationTests<NativeTestsConfig>
    {
    }

    internal class StatusCodeReceivingTests : StatusCodeReceivingTests<NativeTestsConfig>
    {
    }
}
