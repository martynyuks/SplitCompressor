using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace SplitCompressor
{
    public class DecompressorConcatenator : ParallelTask
    {
        public static int PROCESSOR_COUNT = Environment.ProcessorCount;

        public void Run(string srcFile, string outFile)
        {
            _tasks = new List<ArchivePartSubTask>();
            _runnables = new List<Action>();
            List<string> comprFilePartPathes = FilePartSubs.GetAllFilePartPathes(srcFile, true);
            foreach (string path in comprFilePartPathes)
            {
                ArchivePartSubTask task = new DecompressPartSubTask(path, FilePartSubs.FilePathWithoutLastExt(path));
                _tasks.Add(task);
                _runnables.Add(task.Run);
            }
            _runner = new ParallelRunner(_runnables, PROCESSOR_COUNT);
            _runner.Start();
            _runner.WaitCompletion();
            List<string> filePartPathes = FilePartSubs.GetAllFilePartPathes(srcFile, false);
            ConcatenateFiles(filePartPathes, outFile);
            foreach (string filePath in filePartPathes)
            {
                File.Delete(filePath);
            }
        }

        public static void Decompress(string srcFile, string dstFile)
        {
            using (FileStream srcStream = new FileStream(srcFile, FileMode.Open))
            {
                string directory = Path.GetDirectoryName(dstFile);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(dstFile))
                {
                    using (GZipStream decomprStream = new GZipStream(srcStream, CompressionMode.Decompress))
                    {
                        decomprStream.CopyTo(dstStream);
                    }
                }
            }
        }

        public static void DecompressPart(string srcFile, string dstFile)
        {
            using (FileStream srcStream = new FileStream(srcFile, FileMode.Open))
            {
                string directory = Path.GetDirectoryName(dstFile);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(dstFile))
                {
                    using (GZipStream decomprStream = new GZipStream(srcStream, CompressionMode.Decompress))
                    {
                        decomprStream.CopyTo(dstStream);
                    }
                }
            }
        }

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
