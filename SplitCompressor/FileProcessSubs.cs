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

        public static void ClearStream(Stream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
            stream.Seek(0, SeekOrigin.Begin);
        }

        public static void CreateEmptyFile(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
            }
        }
        
        public static void CreateDummyFile(string filePath, long fileSize, int bufSize = 1024 * 1024)
        {
            byte[] dummyBuf = new byte[bufSize];
            int bytesToWrite;
            long posIndex = 0;
            string directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                while (posIndex < fileSize)
                {
                    if (posIndex + bufSize <= fileSize)
                    {
                        bytesToWrite = bufSize;
                    }
                    else
                    {
                        bytesToWrite = (int)(fileSize - posIndex);
                    }
                    fileStream.Write(dummyBuf, 0, bytesToWrite);
                    posIndex += bytesToWrite;
                }
            }
        }
        
        public static void AppendStreamBytesToFile(string filePath, Stream stream)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.Write))
            {
                fileStream.Seek(0, SeekOrigin.End);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

        public static void WriteStreamBytesIntoFile(string filePath, Stream stream, long offset)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.Write))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

    }
}
