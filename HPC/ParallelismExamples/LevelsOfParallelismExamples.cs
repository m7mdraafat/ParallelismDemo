using System.Diagnostics;
using System.Numerics;

namespace ParallelismDemo.Examples;

/// <summary>
/// Demonstrates different Levels of Parallelism
/// </summary>
public static class LevelsOfParallelismExamples
{
    public static async Task RunAllExamples()
    {
        Console.Clear();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("LEVELS OF PARALLELISM");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        // Level 1: Bit-Level Parallelism
        BitLevelParallelism();

        // Level 2: Instruction-Level Parallelism (ILP)
        InstructionLevelParallelism();

        // Level 3: Thread-Level Parallelism
        await ThreadLevelParallelism();

        // Level 4: Process-Level Parallelism
        await ProcessLevelParallelism();
    }

    private static void BitLevelParallelism()
    {
        Console.WriteLine("\n--- Level 1: BIT-LEVEL PARALLELISM ---");
        Console.WriteLine("Operations on multiple bits simultaneously using SIMD\n");

        const int size = 1000000;
        var array1 = new int[size];
        var array2 = new int[size];
        var result = new int[size];

        // Initialize arrays
        for (int i = 0; i < size; i++)
        {
            array1[i] = i;
            array2[i] = i * 2;
        }

        var sw = Stopwatch.StartNew();

        // Scalar operation (one at a time)
        for (int i = 0; i < size; i++)
        {
            result[i] = array1[i] + array2[i];
        }

        sw.Stop();
        Console.WriteLine($"Scalar operation (sequential): {sw.ElapsedMilliseconds}ms");

        sw.Restart();

        // Vector operation (SIMD - multiple elements at once)
        if (Vector.IsHardwareAccelerated)
        {
            int vectorSize = Vector<int>.Count;
            Console.WriteLine($"Vector size (processing {vectorSize} integers simultaneously): {vectorSize}");

            int i = 0;
            for (; i <= size - vectorSize; i += vectorSize)
            {
                var v1 = new Vector<int>(array1, i);
                var v2 = new Vector<int>(array2, i);
                var vResult = v1 + v2;
                vResult.CopyTo(result, i);
            }

            // Handle remaining elements
            for (; i < size; i++)
            {
                result[i] = array1[i] + array2[i];
            }
        }

        sw.Stop();
        Console.WriteLine($"SIMD operation (bit-level parallelism): {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Hardware acceleration enabled: {Vector.IsHardwareAccelerated}");
        Console.WriteLine("\nBit-level parallelism allows processing multiple data elements");
        Console.WriteLine("in a single CPU instruction using SIMD (Single Instruction Multiple Data).");
    }

    private static void InstructionLevelParallelism()
    {
        Console.WriteLine("\n\n--- Level 2: INSTRUCTION-LEVEL PARALLELISM (ILP) ---");
        Console.WriteLine("CPU executes multiple instructions simultaneously\n");

        Console.WriteLine("Modern CPUs use techniques like:");
        Console.WriteLine("  • Pipelining: Breaking instruction execution into stages");
        Console.WriteLine("  • Superscalar execution: Multiple execution units");
        Console.WriteLine("  • Out-of-order execution: Reordering for efficiency");
        Console.WriteLine("  • Branch prediction: Speculative execution\n");

        const int iterations = 10000000;

        // Example showing independent operations that can be executed in parallel
        var sw = Stopwatch.StartNew();

        long a = 0, b = 0, c = 0, d = 0;
        for (int i = 0; i < iterations; i++)
        {
            // These operations are independent and can be executed in parallel by the CPU
            a += i;      // Operation 1
            b += i * 2;  // Operation 2 (independent of Operation 1)
            c += i * 3;  // Operation 3 (independent of Operations 1 & 2)
            d += i * 4;  // Operation 4 (independent of Operations 1, 2 & 3)
        }

        sw.Stop();
        Console.WriteLine($"Four independent operations: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Results: a={a}, b={b}, c={c}, d={d}");

        // Dependent operations (cannot be parallelized at instruction level)
        sw.Restart();

        int x = 0;
        for (int i = 0; i < iterations; i++)
        {
            x = x + i;       // Operation depends on previous result
            x = x * 2;       // Depends on previous operation
            x = x / (i + 1); // Depends on previous operation
        }

        sw.Stop();
        Console.WriteLine($"\nDependent operations (serialized): {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Result: x={x}");
        Console.WriteLine("\nNote: CPU's instruction pipeline handles this automatically.");
    }

    private static async Task ThreadLevelParallelism()
    {
        Console.WriteLine("\n\n--- Level 3: THREAD-LEVEL PARALLELISM (TLP) ---");
        Console.WriteLine("Multiple threads executing concurrently\n");

        int processorCount = Environment.ProcessorCount;
        Console.WriteLine($"Available processor cores: {processorCount}");
        Console.WriteLine($"Current process threads: {Process.GetCurrentProcess().Threads.Count}\n");

        const int workItems = 20;
        var results = new System.Collections.Concurrent.ConcurrentBag<int>();

        var sw = Stopwatch.StartNew();

        // Single-threaded execution
        for (int i = 0; i < workItems; i++)
        {
            PerformWork(i);
        }

        sw.Stop();
        var sequentialTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Single-threaded execution: {sequentialTime}ms");

        sw.Restart();

        // Multi-threaded execution
        var tasks = new List<Task>();
        for (int i = 0; i < workItems; i++)
        {
            int workId = i;
            tasks.Add(Task.Run(() =>
            {
                var result = PerformWork(workId);
                results.Add(result);
                Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Completed work item {workId}");
            }));
        }

        await Task.WhenAll(tasks);

        sw.Stop();
        var parallelTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"\nMulti-threaded execution: {parallelTime}ms");
        Console.WriteLine($"Speedup: {(double)sequentialTime / parallelTime:F2}x");
        Console.WriteLine($"Efficiency: {((double)sequentialTime / parallelTime / processorCount) * 100:F1}%");
    }

    private static async Task ProcessLevelParallelism()
    {
        Console.WriteLine("\n\n--- Level 4: PROCESS-LEVEL PARALLELISM ---");
        Console.WriteLine("Multiple processes executing independently\n");

        Console.WriteLine("Process-level parallelism involves running separate processes that:");
        Console.WriteLine("  • Have their own memory space");
        Console.WriteLine("  • Run independently of each other");
        Console.WriteLine("  • Can run on different machines (distributed computing)");
        Console.WriteLine("  • Communicate via IPC (Inter-Process Communication)\n");

        Console.WriteLine("Current process information:");
        var currentProcess = Process.GetCurrentProcess();
        Console.WriteLine($"  Process ID: {currentProcess.Id}");
        Console.WriteLine($"  Process Name: {currentProcess.ProcessName}");
        Console.WriteLine($"  Threads: {currentProcess.Threads.Count}");
        Console.WriteLine($"  Memory: {currentProcess.WorkingSet64 / 1024 / 1024} MB");

        // Simulate multiple processes working on different tasks
        Console.WriteLine("\nSimulating distributed task processing:");

        var tasks = new[]
        {
            Task.Run(() => SimulateProcessWork("Process-A", "Data Analysis", 300)),
            Task.Run(() => SimulateProcessWork("Process-B", "Report Generation", 250)),
            Task.Run(() => SimulateProcessWork("Process-C", "Backup Operation", 400)),
            Task.Run(() => SimulateProcessWork("Process-D", "Index Rebuild", 350))
        };

        var sw = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"\nAll processes completed in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine("\nNote: In real scenarios, these would be separate OS processes,");
        Console.WriteLine("potentially running on different machines in a cluster.");
    }

    // Helper methods
    private static int PerformWork(int workId)
    {
        Thread.Sleep(100); // Simulate CPU-bound work
        return workId * workId;
    }

    private static async Task SimulateProcessWork(string processName, string taskName, int duration)
    {
        Console.WriteLine($"  [{processName}] Started: {taskName}");
        await Task.Delay(duration);
        Console.WriteLine($"  [{processName}] Completed: {taskName} ({duration}ms)");
    }
}