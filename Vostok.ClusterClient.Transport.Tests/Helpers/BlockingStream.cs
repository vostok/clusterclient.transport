using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    public class BlockingStream : MemoryStream
    {
        private readonly AutoResetEvent autoResetEvent;

        public BlockingStream(byte[] data) : base(data)
        {
            autoResetEvent = new AutoResetEvent(false);
        }

        public void UnblockForOneRead() => autoResetEvent.Set();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            autoResetEvent.WaitOne();
            return await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            autoResetEvent.Dispose();
            base.Dispose(disposing);
        }
    }
}