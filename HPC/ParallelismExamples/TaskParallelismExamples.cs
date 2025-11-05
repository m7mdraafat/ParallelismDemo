using System.Diagnostics;

namespace ParallelismDemo.Examples;

/// <summary>
/// Demonstrates Task Parallelism - executing different operations concurrently
/// </summary>
public static class TaskParallelismExamples
{
    public static async Task RunAllExamples()
    {
        Console.Clear();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("TASK PARALLELISM EXAMPLES");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("\nTask Parallelism: Different operations executed concurrently\n");

        // Example 1: Parallel.Invoke
        ParallelInvokeExample();

        // Example 2: Task-based operations
        await TaskBasedExample();

        // Example 3: Multiple independent tasks
        await MultipleIndependentTasks();

        // Example 4: Task with continuation
        await TaskWithContinuation();
    }

    private static void ParallelInvokeExample()
    {
        Console.WriteLine("\n--- Example 1: Parallel.Invoke ---");
        Console.WriteLine("Executing different methods in parallel\n");

        var sw = Stopwatch.StartNew();

        Parallel.Invoke(
            () => ProcessOrder("ORD-001"),
            () => SendEmail("customer@example.com"),
            () => UpdateInventory("ITEM-123"),
            () => GenerateReport("Monthly Sales")
        );

        sw.Stop();
        Console.WriteLine($"\nAll operations completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task TaskBasedExample()
    {
        Console.WriteLine("\n--- Example 2: Task-based Operations ---");
        Console.WriteLine("Running different async operations concurrently\n");

        var sw = Stopwatch.StartNew();

        // Create tasks for different operations
        var task1 = FetchUserDataAsync("User123");
        var task2 = FetchOrderHistoryAsync("User123");
        var task3 = FetchRecommendationsAsync("User123");

        // Wait for all tasks to complete
        await Task.WhenAll(task1, task2, task3);

        sw.Stop();

        Console.WriteLine($"\nUser Data: {task1.Result}");
        Console.WriteLine($"Order History: {task2.Result}");
        Console.WriteLine($"Recommendations: {task3.Result}");
        Console.WriteLine($"\nAll operations completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task MultipleIndependentTasks()
    {
        Console.WriteLine("\n--- Example 3: Multiple Independent Tasks ---");
        Console.WriteLine("Running various background operations\n");

        var sw = Stopwatch.StartNew();

        var tasks = new[]
        {
            Task.Run(() => CompressFiles("backup.zip")),
            Task.Run(() => ScanForViruses("C:\\Documents")),
            Task.Run(() => OptimizeDatabase("AppDB")),
            Task.Run(() => CleanTempFiles("C:\\Temp"))
        };

        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"\nAll background tasks completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task TaskWithContinuation()
    {
        Console.WriteLine("\n--- Example 4: Task with Continuation ---");
        Console.WriteLine("Chaining tasks with continuations\n");

        var sw = Stopwatch.StartNew();

        var result = await DownloadDataAsync()
            .ContinueWith(t => ProcessDataAsync(t.Result))
            .Unwrap()
            .ContinueWith(t => SaveResultAsync(t.Result))
            .Unwrap();

        sw.Stop();

        Console.WriteLine($"\nFinal result: {result}");
        Console.WriteLine($"Pipeline completed in {sw.ElapsedMilliseconds}ms");
    }

    // Helper methods
    private static void ProcessOrder(string orderId)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Processing order {orderId}...");
        Thread.Sleep(200);
        Console.WriteLine($"  Order {orderId} processed");
    }

    private static void SendEmail(string email)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Sending email to {email}...");
        Thread.Sleep(300);
        Console.WriteLine($"  Email sent to {email}");
    }

    private static void UpdateInventory(string itemId)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Updating inventory for {itemId}...");
        Thread.Sleep(150);
        Console.WriteLine($"  Inventory updated for {itemId}");
    }

    private static void GenerateReport(string reportName)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Generating {reportName} report...");
        Thread.Sleep(400);
        Console.WriteLine($"  {reportName} report generated");
    }

    private static async Task<string> FetchUserDataAsync(string userId)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Fetching user data for {userId}...");
        await Task.Delay(300);
        return $"UserData({userId})";
    }

    private static async Task<string> FetchOrderHistoryAsync(string userId)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Fetching order history for {userId}...");
        await Task.Delay(400);
        return $"5 orders found";
    }

    private static async Task<string> FetchRecommendationsAsync(string userId)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Fetching recommendations for {userId}...");
        await Task.Delay(250);
        return $"10 recommendations";
    }

    private static void CompressFiles(string fileName)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Compressing files to {fileName}...");
        Thread.Sleep(500);
        Console.WriteLine($"  Compression complete: {fileName}");
    }

    private static void ScanForViruses(string path)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Scanning {path} for viruses...");
        Thread.Sleep(450);
        Console.WriteLine($"  Scan complete: No threats found");
    }

    private static void OptimizeDatabase(string dbName)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Optimizing database {dbName}...");
        Thread.Sleep(600);
        Console.WriteLine($"  Database {dbName} optimized");
    }

    private static void CleanTempFiles(string path)
    {
        Console.WriteLine($"  Thread {Environment.CurrentManagedThreadId}: Cleaning temp files in {path}...");
        Thread.Sleep(350);
        Console.WriteLine($"  Temp files cleaned");
    }

    private static async Task<string> DownloadDataAsync()
    {
        Console.WriteLine($"  Step 1: Downloading data...");
        await Task.Delay(300);
        Console.WriteLine($"  Download complete");
        return "RawData";
    }

    private static async Task<string> ProcessDataAsync(string data)
    {
        Console.WriteLine($"  Step 2: Processing {data}...");
        await Task.Delay(400);
        Console.WriteLine($"  Processing complete");
        return "ProcessedData";
    }

    private static async Task<string> SaveResultAsync(string data)
    {
        Console.WriteLine($"  Step 3: Saving {data}...");
        await Task.Delay(200);
        Console.WriteLine($"  Save complete");
        return "Success";
    }
}