using System.Collections.Concurrent;
using System.Diagnostics;

namespace ParallelismDemo.Examples;

/// <summary>
/// Demonstrates common parallel programming patterns
/// </summary>
public static class ParallelPatterns
{
    public static async Task RunAllExamples()
    {
        Console.Clear();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("PARALLEL PROGRAMMING PATTERNS");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        // Pattern 1: Producer-Consumer
        await ProducerConsumerPattern();

        // Pattern 2: Fork-Join
        await ForkJoinPattern();

        // Pattern 3: MapReduce
        await MapReducePattern();

        // Pattern 4: Master-Worker
        await MasterWorkerPattern();
    }

    private static async Task ProducerConsumerPattern()
    {
        Console.WriteLine("\n--- Pattern 1: PRODUCER-CONSUMER ---");
        Console.WriteLine("Multiple producers creating work for multiple consumers\n");

        // queue with automatic waiting - if the queue is full, producers wait; if empty, consumers wait
        var queue = new BlockingCollection<WorkItem>(boundedCapacity: 10);
        var sw = Stopwatch.StartNew();

        // Create producer tasks
        var producers = Enumerable.Range(1, 2).Select(producerId =>
            Task.Run(async () =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    var item = new WorkItem { Id = $"P{producerId}-Item{i}", Data = $"Data from Producer {producerId}" };
                    queue.Add(item);
                    Console.WriteLine($"  [Producer {producerId}] Thread {Environment.CurrentManagedThreadId}: Produced {item.Id}");
                    await Task.Delay(100);
                }
            })
        ).ToArray();

        // Create consumer tasks
        var consumers = Enumerable.Range(1, 3).Select(consumerId =>
            Task.Run(async () =>
            {
                while (!queue.IsCompleted)
                {
                    try
                    {
                        var item = queue.Take();
                        Console.WriteLine($"  [Consumer {consumerId}] Thread {Environment.CurrentManagedThreadId}: Consuming {item.Id}");
                        await Task.Delay(200); // Simulate work
                        Console.WriteLine($"  [Consumer {consumerId}] Finished processing {item.Id}");
                    }
                    catch (InvalidOperationException) { }
                }
            })
        ).ToArray();

        // Wait for all producers to finish
        await Task.WhenAll(producers);
        queue.CompleteAdding();

        // Wait for all consumers to finish
        await Task.WhenAll(consumers);

        sw.Stop();
        Console.WriteLine($"\nProducer-Consumer pattern completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task ForkJoinPattern()
    {
        Console.WriteLine("\n\n--- Pattern 2: FORK-JOIN ---");
        Console.WriteLine("Split work into subtasks, process in parallel, then join results\n");

        var sw = Stopwatch.StartNew();

        // Fork: Split large array into chunks
        var data = Enumerable.Range(1, 100).ToArray();
        int chunkSize = 25;
        var tasks = new List<Task<long>>();

        Console.WriteLine($"Forking: Splitting {data.Length} items into {data.Length / chunkSize} tasks");

        for (int i = 0; i < data.Length; i += chunkSize)
        {
            int start = i;
            int end = Math.Min(i + chunkSize, data.Length);
            int taskId = (i / chunkSize) + 1;

            tasks.Add(Task.Run(() =>
            {
                Console.WriteLine($"  [Task {taskId}] Thread {Environment.CurrentManagedThreadId}: Processing range [{start}..{end})");
                long sum = 0;
                for (int j = start; j < end; j++)
                {
                    sum += data[j];
                }
                Thread.Sleep(200); // Simulate work
                Console.WriteLine($"  [Task {taskId}] Partial sum: {sum}");
                return sum;
            }));
        }

        // Join: Combine results from all tasks
        var results = await Task.WhenAll(tasks);
        long totalSum = results.Sum();

        sw.Stop();

        Console.WriteLine($"\nJoining: Combined results from {tasks.Count} tasks");
        Console.WriteLine($"Total sum: {totalSum}");
        Console.WriteLine($"Fork-Join completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task MapReducePattern()
    {
        Console.WriteLine("\n\n--- Pattern 3: MAPREDUCE ---");
        Console.WriteLine("Map operation transforms data, Reduce aggregates results\n");

        var sw = Stopwatch.StartNew();

        // Sample data: word counts in documents
        var documents = new[]
        {
            "the quick brown fox jumps over the lazy dog",
            "the lazy cat sleeps all day long",
            "quick brown foxes are clever animals",
            "the dog and the cat are friends"
        };

        Console.WriteLine("Input documents:");
        for (int i = 0; i < documents.Length; i++)
        {
            Console.WriteLine($"  Doc {i + 1}: \"{documents[i]}\"");
        }

        // MAP phase: Process each document in parallel
        Console.WriteLine("\n[MAP Phase] Processing documents in parallel...");
        var mapTasks = documents.Select((doc, index) =>
            Task.Run(() =>
            {
                Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Mapping document {index + 1}");
                var words = doc.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var wordCount = new Dictionary<string, int>();

                foreach (var word in words)
                {
                    if (wordCount.ContainsKey(word))
                        wordCount[word]++;
                    else
                        wordCount[word] = 1;
                }

                Thread.Sleep(100); // Simulate work
                return wordCount;
            })
        ).ToArray();

        var mappedResults = await Task.WhenAll(mapTasks);

        // REDUCE phase: Aggregate all results
        Console.WriteLine("\n[REDUCE Phase] Aggregating results...");
        var finalWordCount = new Dictionary<string, int>();

        foreach (var docWordCount in mappedResults)
        {
            foreach (var kvp in docWordCount)
            {
                if (finalWordCount.ContainsKey(kvp.Key))
                    finalWordCount[kvp.Key] += kvp.Value;
                else
                    finalWordCount[kvp.Key] = kvp.Value;
            }
        }

        sw.Stop();

        Console.WriteLine("\nFinal word counts:");
        foreach (var kvp in finalWordCount.OrderByDescending(x => x.Value).Take(10))
        {
            Console.WriteLine($"  '{kvp.Key}': {kvp.Value} occurrences");
        }

        Console.WriteLine($"\nMapReduce completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task MasterWorkerPattern()
    {
        Console.WriteLine("\n\n--- Pattern 4: MASTER-WORKER ---");
        Console.WriteLine("Master distributes work to worker threads and collects results\n");

        var sw = Stopwatch.StartNew();

        // Master creates work items
        var workItems = Enumerable.Range(1, 12).Select(i =>
            new WorkItem { Id = $"Work-{i}", Data = $"Data-{i}" }
        ).ToList();

        Console.WriteLine($"[Master] Created {workItems.Count} work items");

        var workQueue = new ConcurrentQueue<WorkItem>(workItems);
        var results = new ConcurrentBag<string>();
        var workerCount = 4;

        Console.WriteLine($"[Master] Spawning {workerCount} workers\n");

        // Create worker tasks
        var workers = Enumerable.Range(1, workerCount).Select(workerId =>
            Task.Run(async () =>
            {
                Console.WriteLine($"  [Worker {workerId}] Started on Thread {Environment.CurrentManagedThreadId}");

                while (workQueue.TryDequeue(out var item))
                {
                    Console.WriteLine($"  [Worker {workerId}] Processing {item.Id}");
                    await Task.Delay(150); // Simulate work

                    var result = $"{item.Id}-Processed-by-Worker{workerId}";
                    results.Add(result);

                    Console.WriteLine($"  [Worker {workerId}] Completed {item.Id}");
                }

                Console.WriteLine($"  [Worker {workerId}] No more work, shutting down");
            })
        ).ToArray();

        // Master waits for all workers to complete
        await Task.WhenAll(workers);

        sw.Stop();

        Console.WriteLine($"\n[Master] All workers finished");
        Console.WriteLine($"[Master] Collected {results.Count} results");
        Console.WriteLine($"[Master] Total time: {sw.ElapsedMilliseconds}ms");
    }

    private class WorkItem
    {
        public string Id { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}