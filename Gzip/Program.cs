using System;

namespace Gzip
{
    static class Program
    {
        static int Main(string[] args)
        {
            const string compressMode = "compress";
            const string decompressMode = "decompress";

            try
            {
                if (args.Length == 3)
                {
                    switch (args[0].Trim().ToLower())
                    {
                        case compressMode:
                            CompressHelper.CompressByParallelInvoker(args[1].Trim(), args[2].Trim());
                            Console.WriteLine("Сжатие успешно");
                            return 0;
                        case decompressMode:
                            DecompressHelper.UnGzip(args[1].Trim(), args[2].Trim());
                            Console.WriteLine("Распаковка успешна");
                            return 0;
                    }
                }

                Console.WriteLine("Формат вызова:");
                Console.WriteLine(
                    $"GzipTest.exe {compressMode}/{decompressMode} [имя исходного файла] [имя результирующего файла]");

                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }
    }
}