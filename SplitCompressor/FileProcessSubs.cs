using System;
using System.Collections.Generic;
using System.IO;

namespace SplitCompressor
{
    public class FileProcessSubs
    {
        public static void ConcatenateFiles(List<string> filePathes, string outFilePath)
        {
            const int BUF_SIZE = 1024 * 1024;
            byte[] buffer = new byte[BUF_SIZE];
            int bufSizeRead = 0;
            using (FileStream outFileStream = new FileStream(outFilePath, FileMode.Create))
            {
                foreach (string filePath in filePathes)
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        do
                        {
                            bufSizeRead = fileStream.Read(buffer, 0, BUF_SIZE);
                            outFileStream.Write(buffer, 0, bufSizeRead);
                            outFileStream.Flush();
                        }
                        while (bufSizeRead > 0);
                    }
                }
            }
        }

    }
}
