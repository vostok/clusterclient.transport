using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Native
{
    [Explicit]
    internal class AllowAutoRedirectTests : AllowAutoRedirectTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class ClientTimeoutTests : ClientTimeoutTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class ConnectionFailureTests : ConnectionFailureTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class SendFailureTests : SendFailureTests<NativeTestsConfig>
    {
    }

    [Explicit]
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

    [Explicit]
    internal class ContentReceivingTests : ContentReceivingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class ContentSendingTests : ContentSendingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class ContentStreamingTests : ContentStreamingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class HeaderReceivingTests : HeaderReceivingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class HeaderSendingTests : HeaderSendingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class MaxConnectionsPerEndpointTests : MaxConnectionsPerEndpointTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class MethodSendingTests : MethodSendingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class NetworkErrorsHandlingTests : NetworkErrorsHandlingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class ProxyTests : ProxyTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class QuerySendingTests : QuerySendingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class RequestCancellationTests : RequestCancellationTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class StatusCodeReceivingTests : StatusCodeReceivingTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class RemoteCertificateValidationTests : RemoteCertificateValidationTests<NativeTestsConfig>
    {
    }

    [Explicit]
    internal class CredentialsTests : CredentialsTests<NativeTestsConfig>
    {
    }
}