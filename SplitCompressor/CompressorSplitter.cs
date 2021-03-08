using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace SplitCompressor
{
    public class CompressorSplitter
    {
        public static int PROCESSOR_COUNT = Environment.ProcessorCount;

        public static void Run(string srcFile, string dstFile, int partSize)
        {
            List<ArchivePartSubTask> tasks = new List<ArchivePartSubTask>();
            List<Action> runnables = new List<Action>();
            long partCount = FilePartSubs.FilePartCount(srcFile, partSize);
            for (long partIndex = 0; partIndex < partCount; partIndex++)
            {
                ArchivePartSubTask task = new CompressPartSubTask(srcFile, dstFile, partIndex, partCount, partSize);
                tasks.Add(task);
                runnables.Add(task.Run);
            }
            ParallelRunner runner = new ParallelRunner(runnables, PROCESSOR_COUNT);
            runner.Start();
            runner.WaitCompletion();
        }

        public static void Compress(string srcFile, string dstFile)
        {
            using (FileStream srcStream = new FileStream(srcFile, FileMode.Open))
            {
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
            long posIndex = partIndex * partSize;
            using (FileStream srcStream = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                srcStream.Seek(posIndex, SeekOrigin.Begin);
                byte[] buffer = new byte[partSize];
                int bufSize = srcStream.Read(buffer);
                using (FileStream dstStream = File.Create(FilePartPathRegex.ArchivePartPath(dstFile, partIndex, partCount)))
                {
                    using (GZipStream comprStream = new GZipStream(dstStream, CompressionMode.Compress))
                    {
                        comprStream.Write(buffer, 0, bufSize);
                    }
                }
            }
        }

    }
}
