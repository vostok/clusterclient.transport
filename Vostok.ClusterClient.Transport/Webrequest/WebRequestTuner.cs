using System;
using System.Net;
using System.Net.Security;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Commons.Environment;
using Vostok.Commons.Helpers.Network;

namespace Vostok.Clusterclient.Transport.Webrequest
{
    internal static class WebRequestTuner
    {
        private static readonly BindIPEndPoint AddressSniffer = (servicePoint, endPoint, count) =>
        {
            ArpCacheMaintainer.ReportAddress(endPoint.Address);
            return null;
        };

        static WebRequestTuner()
        {
            if (!RuntimeDetector.IsMono)
            {
                HttpWebRequest.DefaultMaximumErrorResponseLength = -1;
                HttpWebRequest.DefaultMaximumResponseHeadersLength = int.MaxValue;

                ServicePointManager.CheckCertificateRevocationList = false;
            }
        }

        public static void Tune(HttpWebRequest request, TimeSpan timeout, WebRequestTransportSettings settings)
        {
            request.ConnectionGroupName = settings.ConnectionGroupName;
            request.Expect = null;
            request.KeepAlive = true;
            request.Pipelined = settings.Pipelined;
            request.Proxy = settings.Proxy;
            request.AllowAutoRedirect = settings.AllowAutoRedirect;
            request.AllowWriteStreamBuffering = false;
            request.AllowReadStreamBuffering = false;
            request.AuthenticationLevel = AuthenticationLevel.None;
            request.AutomaticDecompression = DecompressionMethods.None;
            request.ServerCertificateValidationCallback = settings.RemoteCertificateValidationCallback;

            var servicePoint = request.ServicePoint;

            servicePoint.Expect100Continue = false;
            servicePoint.UseNagleAlgorithm = false;
            servicePoint.ConnectionLimit = settings.MaxConnectionsPerEndpoint;
            servicePoint.MaxIdleTime = (int)settings.ConnectionIdleTimeout.TotalMilliseconds;

            if (settings.TcpKeepAliveEnabled)
            {
                servicePoint.SetTcpKeepAlive(true, (int)settings.TcpKeepAliveTime.TotalMilliseconds, (int)settings.TcpKeepAliveInterval.TotalMilliseconds);
            }

            if (settings.ArpCacheWarmupEnabled)
            {
                if (servicePoint.BindIPEndPointDelegate == null)
                    servicePoint.BindIPEndPointDelegate = AddressSniffer;
            }
            else
            {
                servicePoint.BindIPEndPointDelegate = null;
            }

            if (!RuntimeDetector.IsMono)
                servicePoint.ReceiveBufferSize = Constants.BufferSize;

            var timeoutInMilliseconds = Math.Max(1, (int)timeout.TotalMilliseconds);
            request.Timeout = timeoutInMilliseconds;
            request.ReadWriteTimeout = timeoutInMilliseconds;

            if (settings.ClientCertificates != null)
            {
                foreach (var certificate in settings.ClientCertificates)
                {
                    request.ClientCertificates.Add(certificate);
                }
            }
        }

        public static void Touch()
        {
        }
    }
}
