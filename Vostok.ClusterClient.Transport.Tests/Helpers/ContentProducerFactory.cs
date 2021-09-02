using System.IO;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Transport.Tests.Helpers
{
    internal static class ContentProducerFactory
    {
        public static StreamContentProducer BuildReusableRandomStreamContentProducer(long size, long length) =>
            BuildRandomStreamContentProducer(size, length, true);

        public static StreamContentProducer BuildRandomStreamContentProducer(long size, long length) =>
            BuildRandomStreamContentProducer(size, length, false);

        public static StreamContentProducer BuildReusableRandomStreamContentProducer(long size) =>
            BuildRandomStreamContentProducer(size, null, true);

        public static StreamContentProducer BuildRandomStreamContentProducer(long size) =>
            BuildRandomStreamContentProducer(size, null, false);

        private static StreamContentProducer BuildRandomStreamContentProducer(long size, long? length, bool reusable)
        {
            var content = ThreadSafeRandom.NextBytes(size);

            var contentStream = new MemoryStream(content);

            return new StreamContentProducer(contentStream, length, reusable);
        }
    }
}