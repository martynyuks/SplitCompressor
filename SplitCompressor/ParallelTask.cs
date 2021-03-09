using System;
using System.Collections.Generic;
using System.Text;

namespace SplitCompressor
{
    public abstract class ParallelTask
    {
        protected ParallelRunner _runner;
        protected List<ArchivePartSubTask> _tasks;
        protected List<Action> _runnables;

        public int CompletedCount
        {
            get
            {
                if (_runner != null)
                {
                    return _runner.CompletedCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int TotalCount
        {
            get
            {
                if (_runner != null)
                {
                    return _runner.TotalCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool IsCompleted
        {
            get
            {
                if (_runner != null)
                {
                    return _runner.IsCompleted;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
