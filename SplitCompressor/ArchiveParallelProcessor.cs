using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SplitCompressor
{
    public abstract class ArchiveParallelProcessor
    {
        protected string _srcFilePath;
        protected string _dstFilePath;
        protected int _processorCount;

        protected List<Thread> _threads;
        protected Queue<ArchivePartSubTask> _tasksQueue;
        protected List<ArchivePartSubTask> _tasks;
        protected Mutex _tasksMutex;
        protected Mutex _tasksCompleteMutex;

        protected int _completedCount = 0;
        protected int _totalCount = 0;

        protected ArchiveParallelProcessor(string srcFilePath, string dstFilePath, int processorCount)
        {
            _srcFilePath = srcFilePath;
            _dstFilePath = dstFilePath;
            _processorCount = processorCount;
            _threads = new List<Thread>();
            _tasks = new List<ArchivePartSubTask>();
            _tasksQueue = new Queue<ArchivePartSubTask>();
            _tasksMutex = new Mutex();
            _tasksCompleteMutex = new Mutex();
        }

        public string SrcFilePath
        {
            get { return _srcFilePath; }
        }

        public string DstFilePath
        {
            get { return _dstFilePath; }
        }

        public int CompletedCount
        {
            get { return _completedCount; }
        }

        public int TotalCount
        {
            get { return _totalCount; }
        }

        public bool IsCompleted
        {
            get
            {
                return (_totalCount > 0) ? (_completedCount >= _totalCount) : (false);
            }
        }

        public abstract void Start();

        public void WaitCompletion()
        {
            foreach (Thread thread in _threads)
            {
                thread.Join();
            }
        }
    }
}
