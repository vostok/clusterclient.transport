using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.Tests.Functional.Common;
using Vostok.Commons.Environment;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Universal
{
    internal class AllowAutoRedirectTests : AllowAutoRedirectTests<UniversalTestConfig>
    {
    }

    internal class ClientTimeoutTests : ClientTimeoutTests<UniversalTestConfig>
    {
    }

    internal class ConnectionFailureTests : ConnectionFailureTests<UniversalTestConfig>
    {
    }

    internal class SendFailureTests : SendFailureTests<UniversalTestConfig>
    {
    }

    internal class ConnectionTimeoutTests : ConnectionTimeoutTests<UniversalTestConfig>
    {
        public override void Should_timeout_on_connection_to_a_blackhole_by_connect_timeout()
        {
            while (!Debugger.IsAttached)
            {
            }

            if (!RuntimeDetector.IsDotNetCore20 || RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                base.Should_timeout_on_connection_to_a_blackhole_by_connect_timeout();
        }
    }

    internal class ContentReceivingTests : ContentReceivingTests<UniversalTestConfig>
    {
    }

    internal class ContentSendingTests : ContentSendingTests<UniversalTestConfig>
    {
    }

    internal class ContentStreamingTests : ContentStreamingTests<UniversalTestConfig>
    {
    }

    internal class HeaderReceivingTests : HeaderReceivingTests<UniversalTestConfig>
    {
    }

    internal class HeaderSendingTests : HeaderSendingTests<UniversalTestConfig>
    {
    }

    internal class MaxConnectionsPerEndpointTests : MaxConnectionsPerEndpointTests<UniversalTestConfig>
    {
    }

    internal class MethodSendingTests : MethodSendingTests<UniversalTestConfig>
    {
    }

    internal class NetworkErrorsHandlingTests : NetworkErrorsHandlingTests<UniversalTestConfig>
    {
    }

    internal class ProxyTests : ProxyTests<UniversalTestConfig>
    {
    }

    internal class QuerySendingTests : QuerySendingTests<UniversalTestConfig>
    {
    }

    internal class RequestCancellationTests : RequestCancellationTests<UniversalTestConfig>
    {
    }

    internal class StatusCodeReceivingTests : StatusCodeReceivingTests<UniversalTestConfig>
    {
    }

    internal class RemoteCertificateValidationTests : RemoteCertificateValidationTests<UniversalTestConfig>
    {
    }

    [Explicit]
    internal class CredentialsTests : CredentialsTests<UniversalTestConfig>
    {
    }
}