using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Transport.SystemNetHttp.Contents;

namespace Vostok.Clusterclient.Transport.Tests.SystemNetHttp
{
    [TestFixture]
    internal class CompositeStream_Tests
    {
        private MemoryStream stream1;
        private MemoryStream stream2;
        private MemoryStream stream3;

        private MemoryStream mergedStream;
        private CompositeStream compositeStream;

        [SetUp]
        public void TestSetup()
        {
            var content1 = Guid.NewGuid().ToByteArray();
            var content2 = Guid.NewGuid().ToByteArray();
            var content3 = Guid.NewGuid().ToByteArray();

            stream1 = new MemoryStream(content1);
            stream2 = new MemoryStream(content2);
            stream3 = new MemoryStream(content3);

            mergedStream = new MemoryStream(content1.Concat(content2).Concat(content3).ToArray());

            compositeStream = new CompositeStream(new[] { stream1, stream2, stream3 }, mergedStream.Length);
        }

        [Test]
        public void Should_only_be_able_to_read()
        {
            compositeStream.CanRead.Should().BeTrue();

            compositeStream.CanSeek.Should().BeFalse();
            compositeStream.CanWrite.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(10)]
        [TestCase(16)]
        [TestCase(32)]
        [TestCase(100)]
        public void Should_read_same_as_ordinary_memorystream(int readSize)
        {
            while (compositeStream.Position < compositeStream.Length)
            {
                CompareReads(readSize);
            }

            CompareReads(readSize);
            CompareReads(readSize);
        }

        private void CompareReads(int size)
        {
            var buffer1 = new byte[size + 4];
            var buffer2 = new byte[size + 4];

            var result1 = mergedStream.Read(buffer1, 2, size);
            var result2 = compositeStream.Read(buffer2, 2, size);

            result2.Should().Be(result1);

            compositeStream.Position.Should().Be(mergedStream.Position);

            buffer2.Skip(2).Take(result2).Should().Equal(buffer1.Skip(2).Take(result1));
        }
    }
}
