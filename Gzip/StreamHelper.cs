using System.Collections.Generic;
using System.IO;

namespace Gzip
{
    public static class StreamHelper
    {
        /// <summary>
        /// Разделение GZip файла на чанки
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static IEnumerable<Stream> SplitToChunk(this Stream inputStream)
        {
            var stream = new BufferedStream(inputStream);

            var outputStream = new MemoryStream();

            int currentByte;
            var currentMatch = 0;
            var firstIterate = true;
            while ((currentByte = stream.ReadByte()) >= 0)
            {
                if (firstIterate || currentByte != DecompressHelper.GZipSignature[currentMatch])
                {
                    if (currentMatch > 0)
                    {
                        outputStream.Write(DecompressHelper.GZipSignature, 0, currentMatch);
                        currentMatch = 0;
                    }

                    outputStream.WriteByte((byte) currentByte);

                    firstIterate = false;
                }
                else
                {
                    currentMatch++;
                    if (currentMatch == DecompressHelper.GZipSignature.Length)
                    {
                        outputStream.Position = 0;
                        yield return outputStream;
                        outputStream = new MemoryStream();
                        outputStream.Write(DecompressHelper.GZipSignature);
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