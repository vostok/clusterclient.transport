using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Vostok.Commons.Helpers.Network;

#pragma warning disable 4014

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    internal class TestServer : IDisposable
    {
        private readonly HttpListener listener;
        private volatile ReceivedRequest lastRequest;

        private TestServer(Action<HttpListener> configureListener)
        {
            Port = FreeTcpPortFinder.GetFreePort();
            Host = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Dns.GetHostName() : "localhost";
            listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{Port}/");
            configureListener?.Invoke(listener);
        }

        public static TestServer StartNew(Action<HttpListenerContext> handle, Action<HttpListener> configureListener = null)
        {
            var server = new TestServer(configureListener);

            server.Start(handle);

            return server;
        }

        public ReceivedRequest LastRequest => lastRequest;

        public Uri Url => new Uri($"http://{Host}:{Port}/");

        public int Port { get; }

        public bool BufferRequestBody { get; set; } = true;

        public void Dispose()
        {
            try
            {
                listener.Stop();
                listener.Close();
            }
            catch
            {
                // ignore
            }
        }

        private string Host { get; }

        private void Start(Action<HttpListenerContext> handle)
        {
            listener.Start();

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        var context = await listener.GetContextAsync().ConfigureAwait(false);

                        Task.Run(
                            () =>
                            {
                                Interlocked.Exchange(ref lastRequest, DescribeReceivedRequest(context.Request, context.User?.Identity));

                                handle(context);

                                context.Response.Close();
                            });
                    }
                });
        }

        private ReceivedRequest DescribeReceivedRequest(HttpListenerRequest request, IIdentity userIdentity)
        {
            var receivedRequest = new ReceivedRequest
            {
                Url = request.Url,
                Method = request.HttpMethod,
                Headers = request.Headers,
                Query = HttpUtility.ParseQueryString(request.Url.Query),
            };

            if (BufferRequestBody)
            {
                var bodyStream = new MemoryStream(Math.Max(4, (int)request.ContentLength64));

                request.InputStream.CopyTo(bodyStream);

                receivedRequest.Body = bodyStream.ToArray();
                receivedRequest.BodySize = bodyStream.Length;
            }
            else
            {
                try
                {
                    var buffer = new byte[64 * 1024];

                    while (true)
                    {
                        var bytesReceived = request.InputStream.Read(buffer, 0, buffer.Length);
                        if (bytesReceived == 0)
                            break;

                        receivedRequest.BodySize += bytesReceived;
                    }
                }
                catch (Exception error)
                {
                    Console.Out.WriteLine(error);
                }
            }

            if (userIdentity != null)
            {
                var identity = (HttpListenerBasicIdentity)userIdentity;
                receivedRequest.UserIdentity = new UserIdentity {Name = identity.Name, Password = identity.Password};
            }

            return receivedRequest;
        }
    }
}