#if NET5_0_OR_GREATER
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Vostok.Commons.Helpers.Network;

namespace Vostok.Clusterclient.Transport.Tests.Helpers;

#nullable enable
internal class KestrelTestServer : IDisposable
{
    private readonly IWebHost host;
    private readonly bool useHttps;
    private readonly string hostname;
    private string Scheme => "http" + (useHttps ? "s" : "");
    
    public int Port { get; }
    public Uri Url => new Uri($"{Scheme}://{hostname}:{Port}");

    private KestrelTestServer(Action<HttpContext> handle, Action<ListenOptions>? configureListen = null, bool useHttps = false)
    {
        this.useHttps = useHttps;
        Port = FreeTcpPortFinder.GetFreePort();
        hostname = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Dns.GetHostName() : "localhost";
        host = new WebHostBuilder()
            .UseSockets()
            .UseKestrel(GetDefaultKestrelSetup(Port, configureListen, useHttps))
            .Configure(app => app.Run(context =>
                {
                    handle(context);
                    return Task.CompletedTask;
                }))
            .Build();
    }

    public static async Task<KestrelTestServer> StartNewAsync(Action<HttpContext> handle, Action<ListenOptions>? configureListen = null, bool useHttps = false)
    {
        var server = new KestrelTestServer(handle, configureListen, useHttps);
        await server.StartAsync();
        return server;
    }

    
    private Task StartAsync()
    {
        return host.StartAsync();
    }

    private Task StopAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        return host.StopAsync(cts.Token);
    }

    public void Dispose()
    {
        StopAsync();
    }
    
    private static Action<KestrelServerOptions> GetDefaultKestrelSetup(int port, Action<ListenOptions>? configureListen = null, bool useHttps = false)
    {
        return kestrelOptions => kestrelOptions.ListenAnyIP(port,
            listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                configureListen?.Invoke(listenOptions);

                if (useHttps)
                    listenOptions.UseHttps();
            });
    }
}
#endif