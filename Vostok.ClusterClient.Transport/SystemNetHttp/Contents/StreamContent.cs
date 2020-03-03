using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Contents
{
    internal class StreamContent : GenericContent
    {
        private readonly IStreamContent content;
        private readonly CancellationToken cancellationToken;

        public StreamContent(IStreamContent content, CancellationToken cancellationToken)
        {
            this.content = content;
            this.cancellationToken = cancellationToken;

            Headers.ContentLength = content.Length;
        }

        public override long? Length => content.Length;

        public override Stream AsStream => content.Stream;

        public override async Task Copy(Stream target)
        {
            var bodyStream = new UserStreamWrapper(content.Stream);
            var bytesToSend = content.Length ?? long.MaxValue;
            var bytesSent = 0L;

            using (BufferPool.Default.Rent(Constants.BufferSize, out var buffer))
            {
                while (bytesSent < bytesToSend)
                {
                    var bytesToRead = (int) Math.Min(buffer.Length, bytesToSend - bytesSent);

                    var bytesRead = await bodyStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                        break;

                    await target.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

                    bytesSent += bytesRead;
                }
            }
        }
    }
}
