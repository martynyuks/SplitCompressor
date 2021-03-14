using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public abstract class ArchivePartSubTask
    {
        protected string _srcFile;
        protected string _dstFile;

        protected ArchivePartSubTask(string srcFile, string dstFile)
        {
            _srcFile = srcFile;
            _dstFile = dstFile;
        }

        public abstract void Run();
    }
}
