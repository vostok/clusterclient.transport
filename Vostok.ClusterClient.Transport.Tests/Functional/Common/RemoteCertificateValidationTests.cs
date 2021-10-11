using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class RemoteCertificateValidationTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_use_certificate_validation_callback_if_it_specified_in_settings()
        {
            var calledCount = 0;

            bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                calledCount++;
                return true;
            }

            settings.RemoteCertificateValidationCallback = ValidationCallback;

            var request = Request.Get("https://www.google.com/");
            Send(request);

            calledCount.Should().BeGreaterThan(0);
        }
    }
}