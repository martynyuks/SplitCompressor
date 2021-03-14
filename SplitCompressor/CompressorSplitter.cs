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

        public void Run(string srcFilePath, string dstFilePath, int partSize)
        {
            _tasks = new List<ArchivePartSubTask>();
            _runnables = new List<Action>();
            long partCount = FilePartSubs.FilePartCount(srcFilePath, partSize);
            for (long partIndex = 0; partIndex < partCount; partIndex++)
            {
                ArchivePartSubTask task = new CompressPartSubTask(srcFilePath, dstFilePath, partIndex, partCount, partSize);
                _tasks.Add(task);
                _runnables.Add(task.Run);
            }
            _runner = new ParallelRunner(_runnables, PROCESSOR_COUNT);
            _runner.Start();
            _runner.WaitCompletion();
            List<string> filePartPathes = FilePartSubs.GetAllFilePartPathes(
                FilePartSubs.FilePathWithoutLastExt(dstFilePath), true);
            FileProcessSubs.ConcatenateFiles(filePartPathes, dstFilePath);
            foreach (string filePath in filePartPathes)
            {
                File.Delete(filePath);
            }
        }

        public static void Compress(string srcFilePath, string dstFilePath)
        {
            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open))
            {
                string directory = Path.GetDirectoryName(dstFilePath);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(dstFilePath))
                {
                    using (GZipStream comprStream = new GZipStream(dstStream, CompressionMode.Compress))
                    {
                        srcStream.CopyTo(comprStream);
                    }
                }
            }
        }

        public static void CompressPart(string srcFilePath, string dstFilePath, long partIndex, long partCount, int partSize)
        {
            byte[] buffer = new byte[partSize];
            int bufSize = partSize;
            long posIndex = partIndex * partSize;
            long originalPartSize, compressedPartSize;

            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string directory = Path.GetDirectoryName(dstFilePath);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(FilePartPathRegex.ArchivePartPath(dstFilePath, partIndex, partCount)))
                {
                    dstStream.Write(FilePartHeader.CreateEmptyBuffer());
                    using (GZipStream comprStream = new GZipStream(dstStream, CompressionMode.Compress))
                    {
                        srcStream.Seek(posIndex, SeekOrigin.Begin);
                        bufSize = srcStream.Read(buffer, 0, bufSize);
                        comprStream.Write(buffer, 0, bufSize);
                        originalPartSize = bufSize;
                    }
                }

                using (FileStream dstStream = new FileStream(FilePartPathRegex.ArchivePartPath(dstFilePath, partIndex, partCount), FileMode.Open))
                {
                    compressedPartSize = dstStream.Length - FilePartHeader.SIZE;
                    FilePartHeader header = new FilePartHeader(originalPartSize, compressedPartSize);
                    dstStream.Seek(0, SeekOrigin.Begin);
                    dstStream.Write(header.GetBytes());
                }
            }
        }

    }
}
