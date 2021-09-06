using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;

namespace Vostok.Clusterclient.Transport.SystemNetHttp.Contents
{
    internal class ContentProducerContent : GenericContent
    {
        private readonly IContentProducer contentProducer;
        private readonly CancellationToken cancellationToken;

        public ContentProducerContent(IContentProducer contentProducer, CancellationToken cancellationToken)
        {
            this.contentProducer = contentProducer;
            this.cancellationToken = cancellationToken;

            Headers.ContentLength = contentProducer.Length;
        }

        public override long? Length => contentProducer.Length;

        public override Stream AsStream => throw new NotSupportedException();

        public override Task Copy(Stream target)
        {
            try
            {
                var wrapper = new BufferingStreamWrapper(target);
                return contentProducer.ProduceAsync(wrapper, cancellationToken);
            }
            catch (ContentAlreadyUsedException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (BufferingStreamWrapperException error)
            {
                // ReSharper disable once PossibleNullReferenceException
                throw error.InnerException;
            }
            catch (Exception error)
            {
                throw new UserContentProducerException($"Failed to read from user-provided content producer of type '{contentProducer.GetType().Name}'.", error);
            }
        }
    }
}