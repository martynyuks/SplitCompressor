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

        public void Run(string srcFilePath, string dstFilePath)
        {
            _tasks = new List<ArchivePartSubTask>();
            _runnables = new List<Action>();
            List<FilePartHeader> filePartHeaders = FilePartHeader.GetPartHeadersInComprFile(srcFilePath);
            List<long> filePartOffsets = FilePartHeader.GetPartOffsetsInComprFile(filePartHeaders);
            for (int i = 0; i < filePartOffsets.Count; i++)
            {
                ArchivePartSubTask task = new DecompressPartSubTask(srcFilePath,
                    filePartOffsets[i], filePartHeaders[i].CompressedPartSize,
                    FilePartSubs.FilePathWithoutLastExt(FilePartPathRegex.ArchivePartPath(
                        srcFilePath, i, filePartHeaders.Count)));
                _tasks.Add(task);
                _runnables.Add(task.Run);
            }
            _runner = new ParallelRunner(_runnables, PROCESSOR_COUNT);
            _runner.Start();
            _runner.WaitCompletion();
            List<string> filePartPathes = FilePartSubs.GetAllFilePartPathes(
                FilePartSubs.FilePathWithoutLastExt(srcFilePath), false);
            FileProcessSubs.ConcatenateFiles(filePartPathes, dstFilePath);
            foreach (string filePath in filePartPathes)
            {
                File.Delete(filePath);
            }
        }

        public static void Decompress(string srcFilePath, string dstFilePath)
        {
            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open))
            {
                string directory = Path.GetDirectoryName(dstFilePath);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(dstFilePath))
                {
                    using (GZipStream decomprStream = new GZipStream(srcStream, CompressionMode.Decompress))
                    {
                        decomprStream.CopyTo(dstStream);
                    }
                }
            }
        }

        public static void DecompressPart(string srcFilePath, string dstFilePath)
        {
            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open))
            {
                string directory = Path.GetDirectoryName(dstFilePath);
                Directory.CreateDirectory(directory);
                using (FileStream dstStream = File.Create(dstFilePath))
                {
                    using (GZipStream decomprStream = new GZipStream(srcStream, CompressionMode.Decompress))
                    {
                        decomprStream.CopyTo(dstStream);
                    }
                }
            }
        }

        public static void DecompressPart(string srcFilePath, long srcFilePartOffset,
            long srcFilePartSize, string dstFilePath)
        {
            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] filePartBuf = new byte[srcFilePartSize];
                srcStream.Seek(srcFilePartOffset, SeekOrigin.Begin);
                srcStream.Read(filePartBuf);
                using (MemoryStream filePartStream = new MemoryStream(filePartBuf))
                {
                    string directory = Path.GetDirectoryName(dstFilePath);
                    Directory.CreateDirectory(directory);
                    using (FileStream dstStream = File.Create(dstFilePath))
                    {
                        using (GZipStream decomprStream = new GZipStream(filePartStream, CompressionMode.Decompress))
                        {
                            decomprStream.CopyTo(dstStream);
                        }
                    }
                }
            }
        }

    }
}
