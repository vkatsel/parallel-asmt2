using System.Diagnostics;

namespace parallel_asmt2_vkasel;
using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadPool
{
    private const int MaxQueueSize = 20;
    private int _threadCount;
    
    private readonly Queue<Action> _taskQueue = new Queue<Action>();
    private List<Thread> _threads = new List<Thread>();
    
    private readonly object _lock = new object();

    private bool _isWorking = true;
    private bool _isPaused;

    public int RejectedCount { get; private set; } = 0;
    private long _totalWaitTimeMs = 0;
    private int _waitCycles = 0;
    
    public void Start(int threadNum)
    {
        this._threadCount = threadNum;
        for (int i = 0; i < _threadCount; i++)
        {
            _threads.Add(new Thread(WorkerMethod));
            _threads[i].Start();
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
    }

    public double GetAvgWaitTime()
    {
        lock (_lock)
        {
            if (_waitCycles == 0) return 0;
            return (double)_totalWaitTimeMs / _waitCycles;
        }
    }

    public void AddTask(Action task, int taskId)
    {
        lock (_lock)
        {
            if (_taskQueue.Count >= MaxQueueSize)
            {
                RejectedCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Task {taskId} Rejected");
                return;
            }
            _taskQueue.Enqueue(task);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] Task {taskId} accepted");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[INFO] Queue: {_taskQueue.Count}");
            
            Monitor.Pulse(_lock);
        }
    }

    public void Pause()
    {
        lock (_lock)
        {
            _isPaused = true;
        }
    }

    public void Resume()
    {
        lock (_lock)
        {
            _isPaused = false;
            Monitor.PulseAll(_lock);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            _isWorking = false;
            Monitor.PulseAll(_lock);
        }
    }

    private void WorkerMethod()
    {
        while (true)
        {
            Action task = null;
            lock (_lock)
            {
                var stopwatch = Stopwatch.StartNew();
                while ((_taskQueue.Count == 0 || _isPaused) && _isWorking) 
                {
                    Monitor.Wait(_lock);
                }
                stopwatch.Stop();

                if (stopwatch.ElapsedMilliseconds > 0)
                {
                    _totalWaitTimeMs += stopwatch.ElapsedMilliseconds;
                    _waitCycles++;
                }
                
                if (_taskQueue.Count > 0)
                    task = _taskQueue.Dequeue();
                
                if (!_isWorking && _taskQueue.Count == 0) return;
            }

            if (task != null)
            {
                try
                {
                    task();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] Task failed: {e.Message}");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
            }
        }
    }
}