using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    public class SlowStream : MemoryStream
    {
        public SlowStream(byte[] data) : base(data)
        {
            
        }
        
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
            return await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }
    }
}