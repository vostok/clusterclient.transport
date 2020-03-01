// using NUnit.Framework;
// using Vostok.Clusterclient.Core.Transport;
// using Vostok.Clusterclient.Transport.Tests.Shared.Functional;
// using Vostok.Logging.Abstractions;
// using Vostok.Logging.Console;
//
// namespace Vostok.Clusterclient.Transport.Tests
// {
//     public class Config : ITransportTestConfig
//     {
//        
//         public ITransport CreateTransport(TestTransportSettings settings, ILog log)
//         {
//             var transportSettings = new UniversalTransportSettings
//             {
//                 UseResponseStreaming = settings.UseResponseStreaming,
//                 AllowAutoRedirect = settings.AllowAutoRedirect,
//                 MaxResponseBodySize = settings.MaxResponseBodySize,
//                 BufferFactory = settings.BufferFactory,
//             };
//
//             return new UniversalTransport(transportSettings, log);
//         }
//
//         public TestTransportSettings CreateDefaultSettings() => new TestTransportSettings
//         {
//             MaxConnectionsPerEndpoint = 10 * 1000,
//             BufferFactory = size => new byte[size],
//             UseResponseStreaming = _ => false
//         };
//     }
//
//     [TestFixture]
//     internal class AllowAutoRedirectTests : AllowAutoRedirectTests<Config>
//     {
//     }
//
//     internal class ClientTimeoutTests : ClientTimeoutTests<Config>
//     {
//     }
//
//     internal class ConnectionFailureTests : ConnectionFailureTests<Config>
//     {
//     }
//
//     internal class ConnectionTimeoutTests : ConnectionTimeoutTests<Config>
//     {
//     }
//
//     internal class ContentReceivingTests : ContentReceivingTests<Config>
//     {
//         public override void Should_return_response_with_correct_content_length_when_buffer_factory_is_overridden()
//         {
//             // ignore test: buffer factory setting is non-public and cannot be set from UniversalTransport
//         }
//     }
//
//     internal class ContentSendingTests : ContentSendingTests<Config>
//     {
//     }
//
//     internal class HeaderReceivingTests : HeaderReceivingTests<Config>
//     {
//     }
//
//     internal class HeaderSendingTests : HeaderSendingTests<Config>
//     {
//     }
//
//     internal class MaxConnectionsPerEndpointTests : MaxConnectionsPerEndpointTests<Config>
//     {
//     }
//
//     internal class MethodSendingTests : MethodSendingTests<Config>
//     {
//     }
//
//     // proxy is not configurable in UniversalTransport
//     // internal class ProxyTests : ProxyTests<Config>
//     // {
//     // }
//
//     internal class QuerySendingTests : QuerySendingTests<Config>
//     {
//     }
//
//     internal class RequestCancellationTests : RequestCancellationTests<Config>
//     {
//     }
//
//     internal class StatusCodeReceivingTests : StatusCodeReceivingTests<Config>
//     {
//     }
//
//     internal class NetworkErrorsHandlingTests : NetworkErrorsHandlingTests<Config>
//     {
//     }
// }