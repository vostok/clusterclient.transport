#if NET8_0_OR_GREATER
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common;

internal abstract class Http2Tests<TConfig> : TransportFunctionalTests<TConfig>
    where TConfig : ITransportTestConfig, new()
{
    [Test]
    public void Should_be_capable_of_sending_http2_requests_over_plain_tcp_in_prior_knowledge_mode()
    {
        using var server = KestrelTestServer.StartNew(ctx =>
        {
            ctx.Response.StatusCode = HttpProtocol.IsHttp2(ctx.Request.Protocol) 
                ? StatusCodes.Status200OK 
                : StatusCodes.Status400BadRequest;
        }, configureListen: options => options.Protocols = HttpProtocols.Http2);

        settings.HttpVersion = HttpVersion.Version20;
        settings.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        
        var request = Request.Get(server.Url);
        var response = Send(request);
        response.Code.Should().Be(ResponseCode.Ok);
    }
    
    [Test]
    public void Should_be_capable_of_sending_http2_requests_over_tls_when_upgrade_available()
    {
        using var server = KestrelTestServer.StartNew(ctx =>
        {
            ctx.Response.StatusCode = HttpProtocol.IsHttp2(ctx.Request.Protocol) 
                ? StatusCodes.Status200OK 
                : StatusCodes.Status400BadRequest;
        }, useHttps: true);

        settings.HttpVersion = HttpVersion.Version11;
        settings.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        
        var request = Request.Get(server.Url);
        var response = Send(request);
        response.Code.Should().Be(ResponseCode.Ok);
    }

    [Test]
    public void Should_be_capable_of_reading_trailing_headers()
    {
        const string message = "Hello World";
        const string trailerName = "grpc-status";
        const string trailerValue = "0";
        
        using var server = KestrelTestServer.StartNew(async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            // note(d.khrustalev): we have to send a body, otherwise trailers won't be sent either
            await ctx.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message));
            ctx.Response.AppendTrailer(trailerName, trailerValue);
        }, configureListen: options => options.Protocols = HttpProtocols.Http2);
        
        settings.HttpVersion = HttpVersion.Version20;
        settings.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

        var request = Request.Get(server.Url);
        var response = Send(request);
        response.Code.Should().Be(ResponseCode.Ok);
        response.Content.ToString().Should().Be(message);
        response.Trailers.Should().NotBeNull().And.ContainSingle();
        response.Trailers[trailerName].Should().Be(trailerValue);
    }
}
#endif
