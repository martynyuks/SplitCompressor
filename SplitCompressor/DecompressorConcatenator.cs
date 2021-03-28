using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace SplitCompressor
{
    public class DecompressorConcatenator : ArchiveParallelProcessor
    {
        private long _dstFileSize;
        private long _srcFilePartLargestSize;

        public DecompressorConcatenator(string srcFilePath, string dstFilePath, int processorCount)
            : base(srcFilePath, dstFilePath, processorCount)
        {
        }

        public override void Start()
        {
            List<FilePartHeader> srcPartHeaders = FilePartHeader.GetPartHeadersInComprFile(_srcFilePath);
            List<long> srcPartOffsets = FilePartHeader.GetPartOffsetsInComprFile(srcPartHeaders);
            List<long> dstPartOffsets = FilePartHeader.GetPartOffsetsInOriginFile(srcPartHeaders);
            _srcFilePartLargestSize = FilePartHeader.GetLargestPartSizeInComprFile(srcPartHeaders);
            for (int partIndex = 0; partIndex < srcPartOffsets.Count; partIndex++)
            {
                var task = new DecompressPartSubTask(_srcFilePath,
                    srcPartOffsets[partIndex], srcPartHeaders[partIndex].CompressedPartSize,
                    _dstFilePath, dstPartOffsets[partIndex], srcPartHeaders[partIndex].OriginalPartSize, partIndex);
                _tasks.Add(task);
                _tasksQueue.Enqueue(task);
            }
            for (int i = 0; i < _processorCount; i++)
            {
                Thread thread = new Thread(() =>
                {
                    MemoryStream bufferStream = new MemoryStream();
                    byte[] buffer = new byte[_srcFilePartLargestSize];
                    MemoryStream comprBufferStream = new MemoryStream();
                    while (_completedCount < _totalCount)
                    {
                        _tasksMutex.WaitOne();
                        var task = _tasksQueue.Count > 0 ? _tasksQueue.Dequeue() : null;
                        _tasksMutex.ReleaseMutex();
                        if (task != null)
                        {
                            (task as DecompressPartSubTask).SetAuxiliaryBuffers(bufferStream, buffer, comprBufferStream);
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
                    comprBufferStream.Close();
                });
                _threads.Add(thread);
            }
            _dstFileSize = FilePartHeader.GetTotalOriginalSize(srcPartHeaders);
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

        public static void DecompressPartInMemory(string srcFilePath, long srcFilePartOffset, long srcFilePartSize, long dstPartSize,
            ref MemoryStream bufferStream, ref byte[] buffer, ref MemoryStream comprBufferStream)
        {
            if (buffer.Length < Math.Max(dstPartSize, srcFilePartSize))
            {
                buffer = new byte[Math.Max(dstPartSize, srcFilePartSize)];
            }
            using (FileStream srcStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                srcStream.Seek(srcFilePartOffset, SeekOrigin.Begin);
                srcStream.Read(buffer, 0, (int)srcFilePartSize);

                FileProcessSubs.ClearStream(comprBufferStream);
                comprBufferStream.Write(buffer, 0, (int)srcFilePartSize);
                comprBufferStream.Seek(0, SeekOrigin.Begin);

                FileProcessSubs.ClearStream(bufferStream);
                using (GZipStream decomprStream = new GZipStream(comprBufferStream, CompressionMode.Decompress, true))
                {
                    decomprStream.CopyTo(bufferStream);
                }
            }
        }

    }
}
