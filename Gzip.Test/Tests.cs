using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Gzip.Test
{
    public class Tests
    {
        private MemoryStream _memoryStream;

        [SetUp]
        public void Setup()
        {
            const string @string = "Check splitting demo flow into gzip chunks";

            var array = Encoding.UTF8.GetBytes(@string);

            var span = array.AsSpan();
            var chunk1 = span.Slice(0, 13);
            var chunk2 = span.Slice(13);

            var gZipArray1 = ToGZipArray(chunk1);
            var gZipArray2 = ToGZipArray(chunk2);

            _memoryStream = new MemoryStream();
            _memoryStream.Write(gZipArray1);
            _memoryStream.Write(gZipArray2);
            _memoryStream.Position = 0;
        }

        private static byte[] ToGZipArray(ReadOnlySpan<byte> chunk)
        {
            using (var stream = new MemoryStream(chunk.ToArray()))
            using (var memoryStream = new MemoryStream())
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                stream.CopyTo(gZipStream);
                return memoryStream.ToArray();
            }
        }

        [Test]
        public void TestCountChunk()
        {
            Assert.AreEqual(DecompressHelper.SplitToChunk(_memoryStream).Count(), 2);

            Assert.AreEqual(DecompressHelper.SplitToChunk(Stream.Null).Count(), 0);
        }
    }
}