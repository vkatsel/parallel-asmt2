namespace parallel_asmt2_vkasel;
using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadPool
{
    private const int MaxQueueSize = 20;
    private const int ThreadCount = 6;
    
    private readonly Queue<Action> _taskQueue = new Queue<Action>();
    private List<Thread> _threads = new List<Thread>();
    
    private readonly object _lock = new object();

    private bool _isWorking = true;
    private bool _isPaused = false;

    public ThreadPool()
    {
        for (int i = 0; i < ThreadCount; i++)
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
        
    }

    public void Resume()
    {
        
    }

    public void Stop()
    {
        
    }

    private void WorkerMethod()
    {
        while (true)
        {
            Action task =  null;
            lock (_lock)
            {
                while (_taskQueue.Count == 0 && _isWorking) 
                {
                    Monitor.Wait(_lock);
                }
            }

            if (task != null)
            {
                try
                {
                    task();
                }
                catch (Exception e)
                {
                    
                }
            }
        }
    }
}