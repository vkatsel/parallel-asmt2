namespace parallel_asmt2_vkasel;

class Program
{
    static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        var threadNum = 6;
        var pool = new ThreadPool();
        Console.WriteLine("[INFO] Starting the pool...");
        pool.Start(threadNum);
        Console.WriteLine("[INFO] Adding 30 task to the pool");

        for (int i = 0; i < 30; i++)
        {
            int taskId = i;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[INFO] Adding task {taskId}");
            pool.AddTask(() =>
            {
                int duration = new Random().Next(1000, 5000);
                Thread.Sleep(duration);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[SUCCESS] Task {taskId} completed. Duration: {duration} ms");
            }, taskId);
        }
        Thread.Sleep(5000);
        
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("[INFO] Pausing pool");
        pool.Pause();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[SUCCESS] Pool is paused.");
        pool.AddTask(() => Console.WriteLine("I was added during PAUSE!"), 31);
        Thread.Sleep(10000);

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("[INFO] Resuming pool");
        pool.Resume();
        
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("[INFO] Waiting for some tasks to be completed...");
        Thread.Sleep(3000);

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("[INFO] Stopping pool. Waiting for queued task completion...");
        pool.Stop();
        
        Thread.Sleep(10000);
        
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine();
        Console.WriteLine("[---] General Performance Report [---]");
        Console.WriteLine($"- Total Threads Created: {threadNum}");
        Console.WriteLine($"- Total task rejected: {pool.RejectedCount}");
        Console.WriteLine($"- Average Thread Wait Time: {pool.GetAvgWaitTime():F2} ms");
    }
}