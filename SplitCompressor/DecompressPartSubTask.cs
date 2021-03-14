using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public class DecompressPartSubTask : ArchivePartSubTask
    {
        private long _srcFileOffset;
        private long _srcFileSize;

        public DecompressPartSubTask(string srcFile, long srcFileOffset, long srcFileSize, string dstFile) :
            base(srcFile, dstFile)
        {
            _srcFileOffset = srcFileOffset;
            _srcFileSize = srcFileSize;
        }

        public override void Run()
        {
            DecompressorConcatenator.DecompressPart(_srcFile, _srcFileOffset, _srcFileSize, _dstFile);
        }
    }
}
