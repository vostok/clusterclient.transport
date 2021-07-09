using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Clusterclient.Transport.Webrequest;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Transport
{
    internal class ContentConsumer : IContentConsumer
    {
        private readonly WebRequestState state;
        private readonly CancellationToken cancellationToken;

        public ContentConsumer(WebRequestState state, CancellationToken cancellationToken)
        {
            this.state = state;
            this.cancellationToken = cancellationToken;
        }

        public void Consume(byte[] src, int offset, int count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (count < Constants.LOHObjectSizeThreshold)
                SendSmallBufferedBodyAsync(src, offset, count).ConfigureAwait(false).GetAwaiter().GetResult();
            else
                SendLargeBufferedBodyAsync(src, offset, count).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private Task SendSmallBufferedBodyAsync(byte[] content, int offset, int count)
        {
            return state.RequestStream.WriteAsync(content, offset, count);
        }

        private async Task SendLargeBufferedBodyAsync(byte[] content, int offset, int count)
        {
            using (BufferPool.Default.Rent(Constants.BufferSize, out var buffer))
            {
                var index = offset;
                var end = offset + count;

                while (index < end)
                {
                    var size = Math.Min(buffer.Length, end - index);

                    Buffer.BlockCopy(content, index, buffer, 0, size);

                    await state.RequestStream.WriteAsync(buffer, 0, size).ConfigureAwait(false);

                    index += size;
                }
            }
        }
    }
}