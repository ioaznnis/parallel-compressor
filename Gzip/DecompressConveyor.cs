using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Gzip
{
    /// <summary>
    /// Блочная распаковка файла.
    /// </summary>
    public class DecompressConveyor : ConveyorBase<Stream, Stream>
    {
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;

        public DecompressConveyor(Stream inputStream, Stream outputStream)
        {
            _inputStream = inputStream;
            _outputStream = outputStream;
        }

        public override IEnumerable<Stream> Initialize()
        {
            using (var inputStream = _inputStream)
            {
                foreach (var stream in inputStream.SplitToChunk())
                {
                    yield return stream;
                }
            }
        }

        public override Stream Iterate(Stream chunk)
        {
            var memoryStream = new MemoryStream();

            using (var gZipStream = new GZipStream(chunk, CompressionMode.Decompress))
            {
                gZipStream.CopyTo(memoryStream);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public override void Complete(IEnumerable<Stream> enumerable)
        {
            using (var write = _outputStream)
            {
                foreach (var stream in enumerable)
                {
                    stream.CopyTo(write);
                }
            }
        }
    }
}