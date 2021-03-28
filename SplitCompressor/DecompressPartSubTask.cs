using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SplitCompressor
{
    public class DecompressPartSubTask : ArchivePartSubTask
    {
        private long _srcPartOffset;
        private long _srcPartSize;
        private long _dstPartOffset;
        private long _dstPartSize;

        private MemoryStream _comprBufferStream;

        public DecompressPartSubTask(string srcFilePath, long srcPartOffset, long srcPartSize,
            string dstFilePath, long dstPartOffset, long dstPartSize, long partIndex) :
            base(srcFilePath, dstFilePath)
        {
            _srcPartOffset = srcPartOffset;
            _srcPartSize = srcPartSize;
            _dstPartOffset = dstPartOffset;
            _dstPartSize = dstPartSize;
            _partIndex = partIndex;
        }

        public void SetAuxiliaryBuffers(MemoryStream bufferStream, byte[] buffer, MemoryStream comprBufferStream)
        {
            SetAuxiliaryBuffers(bufferStream, buffer);
            _comprBufferStream = comprBufferStream;
        }

        public override void Run()
        {
            DecompressorConcatenator.DecompressPartInMemory(_srcFilePath, _srcPartOffset, _srcPartSize, _dstPartSize,
                ref _bufferStream, ref _buffer, ref _comprBufferStream);
        }
        
        public override void Complete()
        {
            FileProcessSubs.AppendStreamBytesToFile(_dstFilePath, _bufferStream);
        }
    }
}
