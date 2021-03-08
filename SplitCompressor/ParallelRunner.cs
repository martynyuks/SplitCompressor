using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SplitCompressor
{
    public class ParallelRunner
    {
        private Semaphore _semaphore;
        private List<Action> _runnables;
        private List<Thread> _threads;

        public ParallelRunner(List<Action> runnables, int processorCount)
        {
            _semaphore = new Semaphore(processorCount, processorCount);
            _runnables = runnables;
            _threads = new List<Thread>();
            foreach (Action action in _runnables)
            {
                _threads.Add(new Thread(() =>
                {
                    _semaphore.WaitOne();
                    action();
                    _semaphore.Release();
                }));
            }
        }

        public void Start()
        {
            foreach (Thread thread in _threads)
            {
                thread.Start();
            }
        }

        public void WaitCompletion()
        {
            foreach (Thread thread in _threads)
            {
                thread.Join();
            }
        }
    }
}
