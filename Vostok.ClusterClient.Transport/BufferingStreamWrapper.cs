using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Transport.Helpers;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Transport
{
    internal class BufferingStreamWrapper : Stream
    {
        private readonly Stream target;

        public BufferingStreamWrapper(Stream target)
        {
            this.target = target;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (count < Constants.LOHObjectSizeThreshold)
                    await WriteSmallBufferedBodyAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                else
                    await WriteLargeBufferedBodyAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                throw new BufferingStreamWrapperException(error);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private Task WriteSmallBufferedBodyAsync(byte[] content, int offset, int count, CancellationToken cancellationToken)
        {
            return target.WriteAsync(content, offset, count, cancellationToken);
        }

        private async Task WriteLargeBufferedBodyAsync(byte[] content, int offset, int count, CancellationToken cancellationToken)
        {
            using (BufferPool.Default.Rent(Constants.BufferSize, out var buffer))
            {
                var index = offset;
                var end = offset + count;

                while (index < end)
                {
                    var size = Math.Min(buffer.Length, end - index);

                    Buffer.BlockCopy(content, index, buffer, 0, size);

                    await target.WriteAsync(buffer, 0, size, cancellationToken).ConfigureAwait(false);

                    index += size;
                }
            }
        }

        #region Not supported

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Flush() =>
            throw new NotSupportedException();

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        #endregion
    }
}