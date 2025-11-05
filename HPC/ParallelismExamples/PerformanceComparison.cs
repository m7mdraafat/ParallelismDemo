using System.Diagnostics;

namespace ParallelismDemo.Examples;

/// <summary>
/// Demonstrates performance comparison between sequential and parallel approaches
/// </summary>
public static class PerformanceComparison
{
    public static void RunComparison()
    {
        Console.Clear();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("PERFORMANCE COMPARISON: SEQUENTIAL vs PARALLEL");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        // Scenario 1: Array Processing
        CompareArrayProcessing();

        // Scenario 2: Matrix Multiplication
        CompareMatrixMultiplication();

        // Scenario 3: File Processing
        CompareFileProcessing();
    }

    private static void CompareArrayProcessing()
    {
        Console.WriteLine("\n--- Scenario 1: Array Processing ---");
        Console.WriteLine("Processing 1 million elements\n");

        const int size = 1_000_000;
        var data = Enumerable.Range(1, size).ToArray();
        var results = new int[size];

        // Sequential
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < size; i++)
        {
            results[i] = ExpensiveComputation(data[i]);
        }
        sw.Stop();
        var sequentialTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Sequential execution: {sequentialTime}ms");

        // Parallel
        sw.Restart();
        Parallel.For(0, size, i =>
        {
            results[i] = ExpensiveComputation(data[i]);
        });
        sw.Stop();
        var parallelTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Parallel execution:   {parallelTime}ms");

        DisplayMetrics(sequentialTime, parallelTime);
    }

    private static void CompareMatrixMultiplication()
    {
        Console.WriteLine("\n--- Scenario 2: Matrix Multiplication ---");
        Console.WriteLine("Multiplying 200x200 matrices\n");

        const int size = 200;
        var matrixA = GenerateMatrix(size, size);
        var matrixB = GenerateMatrix(size, size);
        var result = new double[size, size];

        // Sequential
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                double sum = 0;
                for (int k = 0; k < size; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }
                result[i, j] = sum;
            }
        }
        sw.Stop();
        var sequentialTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Sequential execution: {sequentialTime}ms");

        // Parallel
        sw.Restart();
        Parallel.For(0, size, i =>
        {
            for (int j = 0; j < size; j++)
            {
                double sum = 0;
                for (int k = 0; k < size; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }
                result[i, j] = sum;
            }
        });
        sw.Stop();
        var parallelTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Parallel execution:   {parallelTime}ms");

        DisplayMetrics(sequentialTime, parallelTime);
    }

    private static void CompareFileProcessing()
    {
        Console.WriteLine("\n--- Scenario 3: Processing Multiple Files ---");
        Console.WriteLine("Processing 20 simulated files\n");

        var files = Enumerable.Range(1, 20).Select(i => $"file{i}.txt").ToArray();

        // Sequential
        var sw = Stopwatch.StartNew();
        foreach (var file in files)
        {
            ProcessFile(file);
        }
        sw.Stop();
        var sequentialTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Sequential execution: {sequentialTime}ms");

        // Parallel
        sw.Restart();
        Parallel.ForEach(files, file =>
        {
            ProcessFile(file);
        });
        sw.Stop();
        var parallelTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"Parallel execution:   {parallelTime}ms");

        DisplayMetrics(sequentialTime, parallelTime);
    }

    private static void DisplayMetrics(long sequentialTime, long parallelTime)
    {
        var speedup = (double)sequentialTime / parallelTime;
        var efficiency = speedup / Environment.ProcessorCount * 100;

        Console.WriteLine($"\nPerformance Metrics:");
        Console.WriteLine($"  Speedup:    {speedup:F2}x");
        Console.WriteLine($"  Efficiency: {efficiency:F1}% (on {Environment.ProcessorCount} cores)");

        if (speedup < 1)
        {
            Console.WriteLine($"  Note: Parallel version is slower (overhead exceeds benefits)");
        }
        else if (speedup < Environment.ProcessorCount * 0.5)
        {
            Console.WriteLine($"  Note: Limited speedup (possible bottlenecks or overhead)");
        }
        else if (speedup > Environment.ProcessorCount * 0.8)
        {
            Console.WriteLine($"  Note: Excellent speedup (near-linear scaling)");
        }
    }

    // Helper methods
    private static int ExpensiveComputation(int value)
    {
        // Simulate CPU-intensive work
        double result = value;
        for (int i = 0; i < 100; i++)
        {
            result = Math.Sqrt(result + i) * Math.Sin(result);
        }
        return (int)result;
    }

    private static double[,] GenerateMatrix(int rows, int cols)
    {
        var matrix = new double[rows, cols];
        var random = new Random(42);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = random.NextDouble();
            }
        }

        return matrix;
    }

    private static void ProcessFile(string fileName)
    {
        // Simulate file processing
        Thread.Sleep(100);
    }
}