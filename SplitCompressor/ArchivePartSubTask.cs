using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public abstract class ArchivePartSubTask
    {
        protected string _srcFile;
        protected string _dstFile;
        protected long _partIndex;
        protected long _partCount;
        protected int _partSize;

        protected ArchivePartSubTask(string srcFile, string dstFile, long partIndex, long partCount, int partSize)
        {
            _srcFile = srcFile;
            _dstFile = dstFile;
            _partIndex = partIndex;
            _partCount = partCount;
            _partSize = partSize;
        }

        public abstract void Run();
    }
}
