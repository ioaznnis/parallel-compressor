using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Gzip
{
    static class Program
    {
        private const int BufferSize = 1024 * 1024;

        static void Main(string[] args)
        {
            const string path = @"D:\T\2.txt";


            var startNew = Stopwatch.StartNew();
            CompressDefault(path);
            Console.WriteLine(startNew.Elapsed);

            startNew.Restart();
            CompressSequential(path);
            Console.WriteLine(startNew.Elapsed);

            startNew.Restart();
            CompressParallel(path);
            Console.WriteLine(startNew.Elapsed);

            startNew.Restart();
            CompressParallelInvoker(path);
            Console.WriteLine(startNew.Elapsed);

        }

        private static void CompressParallel(string path)
        {
            var compressConveyor = new CompressConveyor(path, $"{path}_Parallel.gz");

            var sequential = compressConveyor.Initialize()
                .AsParallel().AsOrdered()
                .Select(compressConveyor.Iterate)
                .AsSequential();
            compressConveyor.Complete(sequential);
        }

        private static void CompressParallelInvoker(string path)
        {
            var compressConveyor = new CompressConveyor(path, $"{path}_ParallelInvoker.gz");

            var parallelInvoker = ParallelInvokerFactory.Create(compressConveyor);

            parallelInvoker.Invoke();
        }

        private static void CompressSequential(string path)
        {
            var compressConveyor = new CompressConveyor(path, $"{path}_Sequential.gz");
            var enumerable = compressConveyor.Initialize().Select(compressConveyor.Iterate);
            compressConveyor.Complete(enumerable);
        }

        private static void CompressDefault(string path)
        {
            using (var read = File.OpenRead(path))
            using (var write = File.OpenWrite($"{path}.gz"))
            {
                int a;
                var bytes = new byte[BufferSize];

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
    }
}