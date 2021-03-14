using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public class CompressPartSubTask : ArchivePartSubTask
    {
        protected long _partIndex;
        protected long _partCount;
        protected int _partSize;

        public CompressPartSubTask(string srcFile, string dstFile, long partIndex, long partCount, int partSize) :
            base(srcFile, dstFile)
        {
            _partIndex = partIndex;
            _partCount = partCount;
            _partSize = partSize;
        }

        public override void Run()
        {
            CompressorSplitter.CompressPart(_srcFile, _dstFile, _partIndex, _partCount, _partSize);
        }
    }
}
