using System;
using System.Collections.Generic;
using System.IO;

namespace SplitCompressor
{
    public class FilePartHeader
    {
        public const int SIZE = 16;

        private readonly long _originalPartSize;
        public long OriginalPartSize
        {
            get { return _originalPartSize; }
        }

        private readonly long _compressedPartSize;
        public long CompressedPartSize
        {
            get { return _compressedPartSize; }
        }

        public FilePartHeader(long originalPartSize, long compressedPartSize)
        {
            _originalPartSize = originalPartSize;
            _compressedPartSize = compressedPartSize;
        }

        public FilePartHeader(byte[] bytes)
        {
            _originalPartSize = BitConverter.ToInt64(bytes, 0);
            _compressedPartSize = BitConverter.ToInt64(bytes, 8);
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[SIZE];
            Array.Copy(BitConverter.GetBytes(_originalPartSize), 0, bytes, 0, 8);
            Array.Copy(BitConverter.GetBytes(_compressedPartSize), 0, bytes, 8, 8);
            return bytes;
        }

        public static byte[] CreateEmptyBuffer()
        {
            return new byte[SIZE];
        }

        public static List<long> GetPartOffsetsInComprFile(List<FilePartHeader> headers)
        {
            List<long> offsets = new List<long>();
            long offset = 0;
            for (int i = 0; i < headers.Count; i++)
            {
                offset += SIZE;
                offsets.Add(offset);
                offset += headers[i].CompressedPartSize;
            }
            return offsets;
        }

        public static long GetPartOffsetInComprFile(List<FilePartHeader> headers, int partIndex)
        {
            long offset = 0, outOffset = 0;
            for (int i = 0; i < partIndex; i++)
            {
                offset += SIZE;
                outOffset = offset;
                offset += headers[i].CompressedPartSize;
            }
            return outOffset;
        }

        public static List<long> GetPartOffsetsInOriginFile(List<FilePartHeader> headers)
        {
            List<long> offsets = new List<long>();
            long offset = 0;
            for (int i = 0; i < headers.Count; i++)
            {
                offsets.Add(offset);
                offset += headers[i].OriginalPartSize;
            }
            return offsets;
        }

        public static long GetLargestPartSizeInComprFile(List<FilePartHeader> headers)
        {
            long size = 0;
            foreach (FilePartHeader header in headers)
            {
                if (header.CompressedPartSize > size)
                {
                    size = header.CompressedPartSize;
                }
            }
            return size;
        }

        public static long GetLargestPartSizeInOriginFile(List<FilePartHeader> headers)
        {
            long size = 0;
            foreach (FilePartHeader header in headers)
            {
                if (header.OriginalPartSize > size)
                {
                    size = header.OriginalPartSize;
                }
            }
            return size;
        }

        public static long GetTotalOriginalSize(List<FilePartHeader> headers)
        {
            long totalSize = 0;
            foreach (FilePartHeader header in headers)
            {
                totalSize += header.OriginalPartSize;
            }
            return totalSize;
        }

        public static List<FilePartHeader> GetPartHeadersInComprFile(string filePath)
        {
            List<FilePartHeader> headers = new List<FilePartHeader>();
            FilePartHeader header;
            byte[] headerBuf = FilePartHeader.CreateEmptyBuffer();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    fileStream.Read(headerBuf, 0, headerBuf.Length);
                    header = new FilePartHeader(headerBuf);
                    fileStream.Seek(header.CompressedPartSize, SeekOrigin.Current);
                    headers.Add(header);
                }
            }
            return headers;
        }
    
    }
}
