using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Gzip
{
    /// <summary>
    /// Методы распаковки файлов
    /// </summary>
    public static class DecompressHelper
    {
        private static readonly byte[] GZipSignature = {0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a};

        public static void UnGzip(string inputPath, string outputPath)
        {
            using (var read = File.OpenRead(inputPath))
            using (var write = File.OpenWrite(outputPath))
            {
                foreach (var stream in SplitToChunk(read))
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(write);
                    }
                }
            }
        }


        /// <summary>
        /// Разделение GZip файла на чанки
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static IEnumerable<Stream> SplitToChunk(Stream inputStream)
        {
            var stream = new BufferedStream(inputStream);

            var outputStream = new MemoryStream();

            int currentByte;
            var currentMatch = 0;
            var firstIterate = true;
            while ((currentByte = stream.ReadByte()) >= 0)
            {
                if (firstIterate || currentByte != GZipSignature[currentMatch])
                {
                    if (currentMatch > 0)
                    {
                        outputStream.Write(GZipSignature, 0, currentMatch);
                        currentMatch = 0;
                    }

                    outputStream.WriteByte((byte) currentByte);

                    firstIterate = false;
                }
                else
                {
                    currentMatch++;
                    if (currentMatch == GZipSignature.Length)
                    {
                        outputStream.Position = 0;
                        yield return outputStream;
                        outputStream = new MemoryStream();
                        outputStream.Write(GZipSignature);
                        currentMatch = 0;
                    }
                }
            }

            if (outputStream.Length != 0)
            {
                outputStream.Position = 0;
                yield return outputStream;
            }
        }
    }
}