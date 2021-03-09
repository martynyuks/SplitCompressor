using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace SplitCompressor
{
    public class CompressorSplitter : ParallelTask
    {
        public static int PROCESSOR_COUNT = Environment.ProcessorCount;

        public void Run(string srcFile, string dstFile, int partSize)
        {
            _tasks = new List<ArchivePartSubTask>();
            _runnables = new List<Action>();
            long partCount = FilePartSubs.FilePartCount(srcFile, partSize);
            for (long partIndex = 0; partIndex < partCount; partIndex++)
            {
                ArchivePartSubTask task = new CompressPartSubTask(srcFile, dstFile, partIndex, partCount, partSize);
                _tasks.Add(task);
                _runnables.Add(task.Run);
            }
            _runner = new ParallelRunner(_runnables, PROCESSOR_COUNT);
            _runner.Start();
            _runner.WaitCompletion();
        }

        public static void Compress(string srcFile, string dstFile)
        {
            using (FileStream srcStream = new FileStream(srcFile, FileMode.Open))
            {
                string directory = Path.GetDirectoryName(dstFile);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(dstFile))
                {
                    using (GZipStream comprStream = new GZipStream(dstStream, CompressionMode.Compress))
                    {
                        srcStream.CopyTo(comprStream);
                    }
                }
            }
        }

        public static void CompressPart(string srcFile, string dstFile, long partIndex, long partCount, int partSize)
        {
            const int BUF_SIZE = 1024 * 1024;
            byte[] buffer = new byte[BUF_SIZE];
            int bytesRead = 0;
            long posIndex = partIndex * partSize;
            long posIndexEnd = posIndex + partSize;
            using (FileStream srcStream = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (posIndexEnd > srcStream.Length)
                {
                    posIndexEnd = srcStream.Length;
                }
                string directory = Path.GetDirectoryName(dstFile);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(FilePartPathRegex.ArchivePartPath(dstFile, partIndex, partCount)))
                {
                    using (GZipStream comprStream = new GZipStream(dstStream, CompressionMode.Compress))
                    {
                        srcStream.Seek(posIndex, SeekOrigin.Begin);
                        while (posIndex < posIndexEnd)
                        {
                            bytesRead = BUF_SIZE;
                            if (posIndexEnd - posIndex < BUF_SIZE)
                            {
                                bytesRead = (int)(posIndexEnd - posIndex);
                            }
                            bytesRead = srcStream.Read(buffer, 0, bytesRead);
                            comprStream.Write(buffer, 0, bytesRead);
                            posIndex = srcStream.Position;
                        }
                    }
                }

            }
        }

    }
}
