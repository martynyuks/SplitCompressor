using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public class DecompressPartSubTask : ArchivePartSubTask
    {
        public DecompressPartSubTask(string srcFile, string dstFile) :
            base(srcFile, dstFile, -1, -1, -1)
        {
        }

        public override void Run()
        {
            DecompressorConcatenator.DecompressPart(_srcFile, _dstFile);
        }
    }
}
