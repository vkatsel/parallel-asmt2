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
    
    public void Start(int threadNum)
    {
        this._threadCount = threadNum;
        for (int i = 0; i < _threadCount; i++)
        {
            _threads.Add(new Thread(WorkerMethod));
            _threads[i].Start();
        }
    }

    public void AddTask(Action task)
    {
        lock (_lock)
        {
            if (_taskQueue.Count >= MaxQueueSize)
            {
                Console.WriteLine("[INFO] Task Rejected");
                return;
            }
            _taskQueue.Enqueue(task);
            Console.WriteLine("[SUCCESS] Task accepted");
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
                while ((_taskQueue.Count == 0 || _isPaused) && _isWorking) 
                {
                    Monitor.Wait(_lock);
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
                    Console.WriteLine($"[ERROR] Task failed: {e.Message}");
                }
            }
        }
    }
}