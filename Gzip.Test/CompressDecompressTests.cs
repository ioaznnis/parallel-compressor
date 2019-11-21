using System;
using System.IO;
using NUnit.Framework;

namespace Gzip.Test
{
    public class CompressDecompressTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        [TestCase(1024)]
        [TestCase(1024 * 1024 - 42)]
        [TestCase(1024 * 1024 - 1)]
        [TestCase(1024 * 1024 + 1)]
        [TestCase(1024 * 1024 + 42)]
        [TestCase(2 * 1024 * 1024 - 42)]
        [TestCase(2 * 1024 * 1024)]
        [TestCase(2 * 1024 * 1024 + 42)]
        [TestCase(3 * 1024 * 1024 - 42)]
        [TestCase(3 * 1024 * 1024)]
        [TestCase(3 * 1024 * 1024 + 42)]
        [TestCase(4 * 1024 * 1024 - 42)]
        [TestCase(5 * 1024 * 1024)]
        [TestCase(7 * 1024 * 1024 + 42)]

        public void Sequential(int size)
        {
            var bytes = new byte[size];
            var span = bytes.AsSpan();
            new Random().NextBytes(span);
            var inputStream = new MemoryStream();
            inputStream.Write(span);
            inputStream.Position = 0;

            var outputStream = new MemoryStream();

            new CompressConveyor(inputStream, outputStream).SequentialRun();

            //Так как CompressConveyor закрывает поток,
            //а перегрузку добавлять лень,
            //то воспользуемся тем, что ToArray() умеет работать с закрытым потоком
            var compressedStream = new MemoryStream(outputStream.ToArray());

            var decompressStream = new MemoryStream();
            new DecompressConveyor(compressedStream, decompressStream).SequentialRun();

            Assert.True(bytes.AsSpan().SequenceEqual(decompressStream.ToArray().AsSpan()));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(42)]
        [TestCase(1024)]
        [TestCase(1024 * 1024 - 42)]
        [TestCase(1024 * 1024 - 1)]
        [TestCase(1024 * 1024 + 1)]
        [TestCase(1024 * 1024 + 42)]
        [TestCase(2 * 1024 * 1024 - 42)]
        [TestCase(2 * 1024 * 1024)]
        [TestCase(2 * 1024 * 1024 + 42)]
        [TestCase(3 * 1024 * 1024 - 42)]
        [TestCase(3 * 1024 * 1024)]
        [TestCase(3 * 1024 * 1024 + 42)]
        [TestCase(4 * 1024 * 1024 - 42)]
        [TestCase(5 * 1024 * 1024)]
        [TestCase(7 * 1024 * 1024 + 42)]

        public void Parallel(int size)
        {
            var bytes = new byte[size];
            var span = bytes.AsSpan();
            new Random().NextBytes(span);
            var inputStream = new MemoryStream();
            inputStream.Write(span);
            inputStream.Position = 0;

            var outputStream = new MemoryStream();

            new CompressConveyor(inputStream, outputStream).ParallelRun();

            //Так как CompressConveyor закрывает поток,
            //а перегрузку добавлять лень,
            //то воспользуемся тем, что ToArray() умеет работать с закрытым потоком
            var compressedStream = new MemoryStream(outputStream.ToArray());

            var decompressStream = new MemoryStream();
            new DecompressConveyor(compressedStream, decompressStream).ParallelRun();

            Assert.True(bytes.AsSpan().SequenceEqual(decompressStream.ToArray().AsSpan()));
        }
    }
}