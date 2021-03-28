using System;
using System.Collections.Generic;
using System.IO;

namespace SplitCompressor
{
    public abstract class ArchivePartSubTask
    {
        protected string _srcFilePath;
        protected string _dstFilePath;

        protected long _partIndex;
        public long PartIndex
        {
            get { return _partIndex; }
        }
        
        protected MemoryStream _bufferStream;
        protected byte[] _buffer;

        protected ArchivePartSubTask(string srcFilePath, string dstFilePath)
        {
            _srcFilePath = srcFilePath;
            _dstFilePath = dstFilePath;
        }

        public void SetAuxiliaryBuffers(MemoryStream bufferStream, byte[] buffer)
        {
            _bufferStream = bufferStream;
            _buffer = buffer;
        }

        public abstract void Run();

        public abstract void Complete();
    }
}
