using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Network;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    internal class SocketTestServer : IDisposable
    {
        private readonly TcpListener listener;
        private readonly byte[] response;
        private readonly Action<TcpClient> onBeforeRequestReading;

        private SocketTestServer(string response, Action<TcpClient> onBeforeRequestReading)
        {
            this.onBeforeRequestReading = onBeforeRequestReading;
            this.response = Encoding.UTF8.GetBytes(response);

            Port = FreeTcpPortFinder.GetFreePort();
            Host = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Dns.GetHostName() : "localhost";

            listener = new TcpListener(IPAddress.Any, Port);
        }

        public static SocketTestServer StartNew(string response, Action<TcpClient> onBeforeRequestReading = null)
        {
            var server = new SocketTestServer(response, onBeforeRequestReading);

            server.Start();

            return server;
        }

        public Uri Url => new Uri($"http://{Host}:{Port}/");

        public string LastRequest { get; private set; }

        public void Dispose()
        {
            try
            {
                listener.Server.Shutdown(SocketShutdown.Both);
                listener.Stop();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Start()
        {
            listener.Start();

            Task.Run(
                () =>
                {
                    while (true)
                    {
                        var client = listener.AcceptTcpClient();
                        Task.Run(
                            () =>
                            {
                                using (client)
                                {
                                    onBeforeRequestReading?.Invoke(client);
                                    using (var stream = client.GetStream())
                                    {
                                        var request = string.Empty;
                                        var buffer = new byte[4096];
                                        int count;

                                        while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            request += Encoding.UTF8.GetString(buffer, 0, count);

                                            if (request.Contains("\r\n\r\n") || request.Length > 4096)
                                                break;
                                        }

                                        LastRequest = request;

                                        stream.Write(response, 0, response.Length);
                                        stream.Flush();
                                    }
                                    client.Close();
                                }
                            });
                    }
                });
        }

        private string Host { get; }

        private int Port { get; }
    }
}
