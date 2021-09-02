using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;
using Vostok.Clusterclient.Transport.SystemNetHttp.Exceptions;
using Vostok.Clusterclient.Transport.Tests.Helpers;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class ContentProducerContent_Tests
    {
        private MemoryStream stream;
        private long length;

        [SetUp]
        public void TestSetup()
        {
            stream = new MemoryStream(Guid.NewGuid().ToByteArray());
            length = stream.Length;
        }

        [Test]
        public void Should_have_correct_content_length_header()
        {
            var contentProducer = new StreamContentProducer(stream, length);
            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            content.Headers.ContentLength.Should().Be(length);
        }

        [Test]
        public void Should_return_correct_content_when_buffered()
        {
            var contentProducer = new StreamContentProducer(stream, length);
            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            content.ReadAsByteArrayAsync().Result.Should().Equal(stream.ToArray());
        }

        [Test]
        public void Should_not_swallow_cancellation_errors_from_producer()
        {
            var contentProducer = Substitute.For<IContentProducer>();
            contentProducer.ProduceAsync(default, default).ThrowsForAnyArgs(new TaskCanceledException());

            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            Action action = () => content.CopyToAsync(Stream.Null).GetAwaiter().GetResult();

            action.Should().Throw<TaskCanceledException>();
        }

        [Test]
        public void Should_not_swallow_cancellation_errors_from_writing_stream()
        {
            var contentProducer = new StreamContentProducer(stream, length);

            var writingStream = Substitute.For<Stream>();
            writingStream.WriteAsync(default, default, default, default).ThrowsForAnyArgs(new TaskCanceledException());

            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            Action action = () => content.CopyToAsync(writingStream).GetAwaiter().GetResult();

            action.Should().Throw<TaskCanceledException>();
        }

        [Test]
        public void Should_not_swallow_content_already_used_errors()
        {
            var error = new ContentAlreadyUsedException("I fill used");

            var contentProducer = Substitute.For<IContentProducer>();
            contentProducer.ProduceAsync(default, default).ThrowsForAnyArgs(error);

            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            Action action = () => content.CopyToAsync(Stream.Null).GetAwaiter().GetResult();

            action.Should().Throw<ContentAlreadyUsedException>().Which.Should().BeSameAs(error);
        }

        [Test]
        public void Should_wrap_generic_producing_errors_into_user_content_producer_exceptions()
        {
            var error = new Exception("Producing failed!");

            var contentProducer = Substitute.For<IContentProducer>();
            contentProducer.ProduceAsync(default, default).ThrowsForAnyArgs(error);

            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            Action action = () => content.CopyToAsync(Stream.Null).GetAwaiter().GetResult();

            action.Should().Throw<UserContentProducerException>().Which.InnerException.Should().BeSameAs(error);
        }

        // note (patrofimov, 14.07.2021): В целом, может подлежать дискуссии, является ли ошибка, произошедшая при записи тела запроса в поток, ошибкой продьюсинга контента.
        //                                Другими словами, нужно ли отделять BodySendException или можно просто сказать, что все generic ошибки внутри продьюсинга - это UserContentProducerException.
        //                                В общем случае, неизвестно, каким образом юзер продьюсит контент. Может быть так, что продьюсинг контента корректный, но транспорт не может отправить запрос по обстоятельствам, не зависящим от юзера.
        //                                Либо наоборот - отправка исправна, но в процессе продьюсинга кастомный баг. Поэтому сделан выбор в пользу различия этих двух ситуаций.
        [Test]
        public void Should_wrap_generic_writing_errors_into_body_send_exceptions()
        {
            var contentProducer = new StreamContentProducer(stream, length);
            var content = new ContentProducerContent(contentProducer, CancellationToken.None);

            var error = new Exception("Sending body failed!");

            var targetStream = Substitute.For<Stream>();
            targetStream.WriteAsync(default, default, default, default).ThrowsForAnyArgs(error);

            Action action = () => content.CopyToAsync(targetStream).GetAwaiter().GetResult();

            action.Should().Throw<BodySendException>().Which.InnerException.Should().BeSameAs(error);
        }
    }
}