using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    public class BlockingOnNonZeroOffsetStream : MemoryStream
    {
        private readonly TaskCompletionSource<object> completionSource;

        public BlockingOnNonZeroOffsetStream(byte[] data, TaskCompletionSource<object> completionSource) : base(data)
        {
            this.completionSource = completionSource;
        }
        
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (offset > 0)
            {
                await completionSource.Task;
            }
            return await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }
    }
}