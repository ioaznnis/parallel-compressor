using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Gzip
{
    /// <summary>
    /// Блочное сжатие файла.
    /// </summary>
    public class CompressConveyor : ConveyorBase<byte[], Stream>
    {
        private readonly Stream _inputStream;
        private readonly Stream _outputStream;

        private const int BufferSize = 1024 * 1024;

        public CompressConveyor(Stream inputStream, Stream outputStream)
        {
            _inputStream = inputStream;
            _outputStream = outputStream;
        }

        /// <summary>
        /// Чтение файла блоками.
        /// <remarks>Если использовать <see langword="break"/> при работе с итератором,
        /// то <see cref="IDisposable.Dispose"/> у файлового потока не вызывается,
        /// так как <see langword="finally"/> исполняется только после полного прохода по <see cref="IEnumerable{T}"/>
        /// или ручного вызова <see cref="IEnumerable{T}.GetEnumerator"/>, а затем <see cref="IDisposable.Dispose"/>.
        /// Про это надо помнить или передавать поток извне...</remarks>
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<byte[]> Initialize()
        {
            using (var read = _inputStream)
            {
                int readLength;
                var buffer = new byte[BufferSize];

                while ((readLength = read.Read(buffer)) > 0)
                {
                    //Воспользуемся span, что бы обрезать буфер, что бы буфер всегда был полный.
                    //В идеале бы все на Span/Memory переписать, но сразу не взлетело, так как параллельность
                    yield return buffer.AsSpan(0, readLength).ToArray();
                }
            }
        }

        /// <summary>
        /// Сжатие блока
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public override Stream Iterate(byte[] chunk)
        {
            var memoryStream = new MemoryStream();

            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(chunk);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        /// Запись блоков в файл
        /// </summary>
        /// <param name="enumerable"></param>
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