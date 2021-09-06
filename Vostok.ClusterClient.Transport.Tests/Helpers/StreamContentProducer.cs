using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    internal class StreamContentProducer : IContentProducer
    {
        internal readonly MemoryStream data;

        public StreamContentProducer(MemoryStream data, long? length = null, bool reusable = false)
        {
            this.data = data;
            IsReusable = reusable;
            Length = length;
        }

        public Task ProduceAsync(Stream requestStream, CancellationToken cancellationToken)
        {
            data.CopyTo(requestStream);

            if (IsReusable)
                data.Seek(0, SeekOrigin.Current);

            return Task.CompletedTask;
        }
        
        public bool IsReusable { get; }
        public long? Length { get; }
    }
}