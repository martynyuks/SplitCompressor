using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace SplitCompressor
{
    public class CompressorSplitter : ArchiveParallelProcessor
    {
        private int _partSize;

        public CompressorSplitter(string srcFilePath, string dstFilePath, int partSize, int processorCount)
            : base(srcFilePath, dstFilePath, processorCount)
        {
            _partSize = partSize;
        }

        public override void Start()
        {
            long partCount = FilePartSubs.FilePartCount(_srcFilePath, _partSize);
            for (long partIndex = 0; partIndex < partCount; partIndex++)
            {
                var task = new CompressPartSubTask(_srcFilePath, _dstFilePath, _partSize, partIndex);
                _tasks.Add(task);
                _tasksQueue.Enqueue(task);
            }
            for (int i = 0; i < _processorCount; i++)
            {
                Thread thread = new Thread(() =>
                {
                    MemoryStream bufferStream = new MemoryStream();
                    byte[] buffer = new byte[_partSize];
                    while (_completedCount < _totalCount)
                    {
                        _tasksMutex.WaitOne();
                        var task = _tasksQueue.Count > 0 ? _tasksQueue.Dequeue() : null;
                        _tasksMutex.ReleaseMutex();
                        if (task != null)
                        {
                            task.SetAuxiliaryBuffers(bufferStream, buffer);
                            task.Run();
                            while (_completedCount < task.PartIndex)
                            { }
                            _tasksCompleteMutex.WaitOne();
                            task.Complete();
                            _tasksCompleteMutex.ReleaseMutex();
                            _completedCount++;
                        }
                    }
                    bufferStream.Close();
                });
                _threads.Add(thread);
            }
            _completedCount = 0;
            _totalCount = _tasks.Count;

            FileProcessSubs.CreateEmptyFile(_dstFilePath);

            foreach (Thread thread in _threads)
            {
                thread.Start();
            }
        }

        public void Run()
        {
            Start();
            WaitCompletion();
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
                    using (GZipStream comprStream = new GZipStream(dstStream, CompressionMode.Compress, true))
                    {
                        srcStream.Seek(posIndex, SeekOrigin.Begin);
                        bufSize = srcStream.Read(buffer, 0, bufSize);
                        comprStream.Write(buffer, 0, bufSize);
                        originalPartSize = bufSize;
                    }
                    compressedPartSize = dstStream.Length - FilePartHeader.SIZE;
                    FilePartHeader header = new FilePartHeader(originalPartSize, compressedPartSize);
                    dstStream.Seek(0, SeekOrigin.Begin);
                    dstStream.Write(header.GetBytes());
                }
            }
        }

        public static void CompressPartInMemory(string srcFilePath, long partIndex, int partSize,
            ref MemoryStream bufferStream, ref byte[] buffer)
        {
            if (buffer.Length < partSize) buffer = new byte[partSize];
            int bufSize = partSize;
            long posIndex = partIndex * partSize;
            long originalPartSize, compressedPartSize;

            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                FileProcessSubs.ClearStream(bufferStream);
                bufferStream.Write(FilePartHeader.CreateEmptyBuffer());
                using (GZipStream comprStream = new GZipStream(bufferStream, CompressionMode.Compress, true))
                {
                    srcStream.Seek(posIndex, SeekOrigin.Begin);
                    bufSize = srcStream.Read(buffer, 0, bufSize);
                    comprStream.Write(buffer, 0, bufSize);
                    originalPartSize = bufSize;
                }
                compressedPartSize = bufferStream.Length - FilePartHeader.SIZE;
                FilePartHeader header = new FilePartHeader(originalPartSize, compressedPartSize);
                bufferStream.Seek(0, SeekOrigin.Begin);
                bufferStream.Write(header.GetBytes());
            }
        }

    }
}
