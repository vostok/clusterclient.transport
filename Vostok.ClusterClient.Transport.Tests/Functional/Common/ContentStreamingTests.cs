using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.Functional.Common
{
    internal abstract class ContentStreamingTests<TConfig> : TransportFunctionalTests<TConfig>
        where TConfig : ITransportTestConfig, new()
    {
        [Test]
        public void Should_send_large_request_body()
        {
            const long size = 5L * 1024 * 1024 * 1024;

            using (var server = TestServer.StartNew(ctx => { ctx.Response.StatusCode = 200; }))
            {
                server.BufferRequestBody = false;

                var response = Send(Request.Post(server.Url).WithContent(new LargeStream(size)), 10.Minutes());

                response.EnsureSuccessStatusCode();

                server.LastRequest.BodySize.Should().Be(size);
            }
        }

        [Test]
        public void Should_read_large_response_body()
        {
            var serverBuffer = new byte[1024 * 1024];
            var clientBuffer = new byte[1024 * 1024];

            var iterations = 5 * 1024;

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    for (var i = 0; i < iterations; ++i)
                        ctx.Response.OutputStream.Write(serverBuffer, 0, serverBuffer.Length);
                }))
            {
                settings.UseResponseStreaming = _ => true;

                var response = Send(Request.Get(server.Url), 10.Minutes());

                response.EnsureSuccessStatusCode();

                long count = 0;

                using (var stream = response.Stream)
                {
                    while (true)
                    {
                        var c = stream.Read(clientBuffer, 0, clientBuffer.Length);
                        if (c == 0)
                            break;
                        count += c;
                    }
                }

                count.Should().Be((long)iterations * serverBuffer.Length);
            }
        }

        #region LargeStream

        private class LargeStream : Stream
        {
            private readonly long length;
            private long read;

            public LargeStream(long length)
            {
                this.length = length;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var r = (int)Math.Max(0, Math.Min(count, length - read));
                read += r;
                return r;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        #endregion
    }
}
