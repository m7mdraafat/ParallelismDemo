
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ParallelismDemo.Examples
{
    public static class DataParallelismExamples
    {
        public static async Task RunAllExamples()
        {
            Console.Clear();
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine("DATA PARALLELISM EXAMPLES");
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine("\nData Parallelism: Same operation on different data elements simultaneously\n");

            // EX1: Parallel.For
            ParallelForExample();

            // EX2: Parallel.ForEach
            ParallelForEachExample();

            // EX3: PLINQ (Parallel LINQ)
            PLINQExample();

            // EX4: Parallel Array Processing
            await ParallelArrayProcessingExample();
        }

        private static void ParallelForExample()
        {
            Console.WriteLine("\n--- Example 1: Parallel.For ---");
            Console.WriteLine("Processing array indices in parallel\n");

            const int size = 10;
            int[] numbers = new int[size];

            var sw = Stopwatch.StartNew();

            // Sequential version
            for (int i =0; i < size; i++)
            {
                numbers[i] = ComputeSquare(i);
            }

            sw.Stop();
            Console.WriteLine($"Sequential execution: {sw.ElapsedMilliseconds}ms");

            sw.Restart();

            // Parallel version
            Parallel.For(0, size, i =>
            {
                numbers[i] = ComputeSquare(i);
                Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Processing index {i} -> {numbers[i]}");
            });

            sw.Stop();
            Console.WriteLine($"Parallel execution: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Result: [{string.Join(", ", numbers)}]");
        }

        private static void ParallelForEachExample()
        {
            Console.WriteLine("\n--- Example 2: Parallel.ForEach---");
            Console.WriteLine("Processing collection items in parallel\n");

            var cities = new List<string>
            {
                "New York", "London", "Paris", "Sydney",
                "Dubai", "Cairo", "Mumbai", "Berlin", "Toronto"
            };

            var results = new ConcurrentBag<string>();

            Parallel.ForEach(cities, city =>
            {
                var processed = ProcessCity(city);
                results.Add(processed);
                Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Processed {city}");
            });

            Console.WriteLine($"\nProcessed {results.Count} cities");
        }

        private static void PLINQExample()
        {
            Console.WriteLine("\n--- Example 3: PLINQ (Parallel LINQ) ---");
            Console.WriteLine("Using PLINQ for parallel queries\n");

            var numbers = Enumerable.Range(1, 20).ToList();

            var sw = Stopwatch.StartNew();

            // Sequential LINQ
            var sequentialResult = numbers
                .Where(n => n % 2 == 0)
                .Select(n => n * n)
                .ToList();

            sw.Stop();

            Console.WriteLine($"Sequential LINQ: {sw.ElapsedMilliseconds}ms");

            sw.Restart();

            // Parallel LINQ
            var parallelResult = numbers
                .AsParallel()
                .WithDegreeOfParallelism(10)
                .Where(n => n % 2 == 0)
                .Select(n =>
                {
                    Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Processing {n}");
                    return n * n;
                })
                .ToList();

            sw.Stop();

            Console.WriteLine($"Parallel LINQ: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Result: [{string.Join(", ", parallelResult)}]");
        }

        private static async Task ParallelArrayProcessingExample()
        {
            Console.WriteLine("\n--- Example 4: Parallel Array processing with Tasks ---");
            Console.WriteLine("Processing Large dataset with Task Parallel Library\n");

            int[] data = Enumerable.Range(1, 1000).ToArray();
            int chunkSize = 250;
            var tasks = new List<Task<long>>();

            var sw = Stopwatch.StartNew();

            // Divide data into chunks and process in parallel
            for (int i = 0; i < data.Length; i+= chunkSize)
            {
                int start = i;
                int end = Math.Min(i + chunkSize, data.Length);

                tasks.Add(Task.Run(() => ProcessChunk(data, start, end)));
            }

            var results = await Task.WhenAll(tasks);
            sw.Stop();

            long total = results.Sum();
            Console.WriteLine($"Processed {data.Length} elements in {tasks.Count} chunks");
            Console.WriteLine($"Total sum: {total}");
            Console.WriteLine($"Execution time: {sw.ElapsedMilliseconds}ms");
        }


        // Helper methods
        private static int ComputeSquare(int n)
        {
            Thread.Sleep(100); // Simulate work
            return n * n;
        }

        private static string ProcessCity(string city)
        {
            Thread.Sleep(100); // Simulate processing city
            return $"{city} (processed)";
        }

        private static long ProcessChunk(int[] data, int start, int end)
        {
            Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Processing chunks [{start}..{end}]");
            long sum = 0;
            for (int i = start; i < end; i++)
            {
                sum += data[i];
            }

            Thread.Sleep(50);
            return sum;
        }
    }
}
