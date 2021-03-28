using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SplitCompressor
{
    public class CompressPartSubTask : ArchivePartSubTask
    {
        private int _partSize;

        public CompressPartSubTask(string srcFilePath, string dstFilePath, int partSize, long partIndex) :
            base(srcFilePath, dstFilePath)
        {
            _partIndex = partIndex;
            _partSize = partSize;
        }

        public override void Run()
        {
            CompressorSplitter.CompressPartInMemory(_srcFilePath, _partIndex, _partSize,
                ref _bufferStream, ref _buffer);
        }

        public override void Complete()
        {
            FileProcessSubs.AppendStreamBytesToFile(_dstFilePath, _bufferStream);
        }
    }
}
