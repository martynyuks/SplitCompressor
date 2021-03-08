using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public class CompressPartSubTask : ArchivePartSubTask
    {
        public CompressPartSubTask(string srcFile, string dstFile, long partIndex, long partCount, int partSize) :
            base(srcFile, dstFile, partIndex, partCount, partSize)
        {
        }

        public override void Run()
        {
            CompressorSplitter.CompressPart(_srcFile, _dstFile, _partIndex, _partCount, _partSize);
        }
    }
}
