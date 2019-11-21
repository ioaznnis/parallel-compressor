using System.IO;
using System.IO.Compression;

namespace Gzip
{
    /// <summary>
    /// Методы распаковки файлов
    /// </summary>
    public static class DecompressHelper
    {
        public static readonly byte[] GZipSignature = {0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a};
        
        /// <summary>
        /// Распаковка файла, основанная на потоках
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void UnGzip(string inputPath, string outputPath)
        {
            using (var read = File.OpenRead(inputPath))
            using (var write = File.OpenWrite(outputPath))
            {
                foreach (var stream in read.SplitToChunk())
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(write);
                    }
                }
            }
        }

        /// <summary>
        /// Распаковка файла, основанная на использовании собственной реализации многопоточности
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void UnGzipByParallelInvoker(string inputPath, string outputPath)
        {
            new DecompressConveyor(File.OpenRead(inputPath), File.OpenWrite(outputPath)).ParallelRun();
        }

    }
}