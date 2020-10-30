using System.IO;
using System.Threading.Tasks;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Transport.Sockets
{
    internal class SocketTuningContent : GenericContent
    {
        private readonly GenericContent content;
        private readonly SocketTuner tuner;
        private readonly ILog log;

        public SocketTuningContent(GenericContent content, SocketTuner tuner, ILog log)
        {
            this.content = content;
            this.tuner = tuner;
            this.log = log;

            CopyHeaders();
        }

        public override long? Length => content.Length;

        public override Stream AsStream => content.AsStream;

        public override Task Copy(Stream target)
        {
            tuner.Tune(SocketAccessor.GetSocket(target, log));

            return content.Copy(target);
        }

        private void CopyHeaders()
        {
            if (ContentHeadersHelper.TryCopyByReference(content, this))
                return;
            
            foreach (var pair in content.Headers)
                Headers.Add(pair.Key, pair.Value);
        }
    }
}
