using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Gzip
{
    /// <summary>
    /// Варианты сжатия файлов через Gzip.
    /// <remarks>Все реализации позволяют сжимать файлы любых размеров.
    /// Потребление памяти O(1), если не учитывать реализацию <see cref="GZipStream"/></remarks>
    /// </summary>
    public static class CompressHelper
    {
        /// <summary>
        /// Сжатие файла, основанное на использовании собственной реализации многопоточности. Параллельное
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void CompressByParallelInvoker(string inputPath, string outputPath)
        {
            new CompressConveyor(File.OpenRead(inputPath), File.OpenWrite(outputPath)).ParallelRun();
        }

        /// <summary>
        /// Сжатие файла, основанное на использовании LINQ. Параллельное
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void CompressParallel(string inputPath, string outputPath)
        {
            var compressConveyor = new CompressConveyor(File.OpenRead(inputPath), File.OpenWrite(outputPath));

            var sequential = compressConveyor.Initialize()
                .AsParallel().AsOrdered()
                .Select(compressConveyor.Iterate)
                .AsSequential();
            compressConveyor.Complete(sequential);
        }

        /// <summary>
        /// Сжатие файла, основанное на использовании LINQ. Не параллельное
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void CompressSequential(string inputPath, string outputPath)
        {
            new CompressConveyor(File.OpenRead(inputPath), File.OpenWrite(outputPath)).SequentialRun();
        }

        /// <summary>
        /// Сжатие файла, основанное на чтении из файла блоков в цикле. Не параллельное
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void CompressDefaultChunk(string inputPath, string outputPath)
        {
            const int bufferSize = 1024 * 1024;

            using (var read = File.OpenRead(inputPath))
            using (var write = File.OpenWrite(outputPath))
            {
                int a;
                var bytes = new byte[bufferSize];

                while ((a = read.Read(bytes)) > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                        {
                            gZipStream.Write(bytes, 0, a);
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(write);
                    }
                }
            }
        }

        /// <summary>
        /// Сжатие файла, основанное на потоках. Позволяет сжимать файлы любых размеров. Не параллельное
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public static void CompressDefault(string inputPath, string outputPath)
        {
            using (var read = File.OpenRead(inputPath))
            using (var write = File.OpenWrite(outputPath))
            using (var gZipStream = new GZipStream(write, CompressionMode.Compress, true))
            {
                read.CopyTo(gZipStream);
            }
        }
    }
}