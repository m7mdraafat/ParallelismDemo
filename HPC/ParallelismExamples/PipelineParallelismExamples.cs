using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace ParallelismDemo.Examples;

/// <summary>
/// Demonstrates Pipeline Parallelism - data flows through multiple stages of processing
/// </summary>
public static class PipelineParallelismExamples
{
    public static async Task RunAllExamples()
    {
        Console.Clear();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("PIPELINE PARALLELISM EXAMPLES");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("\nPipeline Parallelism: Data flows through multiple processing stages\n");

        // Example 1: TPL Dataflow Pipeline
        await DataflowPipelineExample();

        // Example 2: Channel-based Pipeline
        await ChannelBasedPipelineExample();

        // Example 3: Image Processing Pipeline
        await ImageProcessingPipeline();
    }

    private static async Task DataflowPipelineExample()
    {
        Console.WriteLine("\n--- Example 1: TPL Dataflow Pipeline ---");
        Console.WriteLine("Processing data through multiple transformation stages\n");

        var sw = Stopwatch.StartNew();

        // Stage 1: Generate data
        var generateBlock = new TransformBlock<int, string>(
            async id =>
            {
                await Task.Delay(50);
                var data = $"Data-{id}";
                Console.WriteLine($"  [Generate] Thread {Environment.CurrentManagedThreadId}: Generated {data}");
                return data;
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Stage 2: Process data
        var processBlock = new TransformBlock<string, string>(
            async data =>
            {
                await Task.Delay(100);
                var processed = $"{data}-Processed";
                Console.WriteLine($"  [Process] Thread {Environment.CurrentManagedThreadId}: Processed {processed}");
                return processed;
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Stage 3: Validate data
        var validateBlock = new TransformBlock<string, string>(
            async data =>
            {
                await Task.Delay(50);
                var validated = $"{data}-Valid";
                Console.WriteLine($"  [Validate] Thread {Environment.CurrentManagedThreadId}: Validated {validated}");
                return validated;
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Stage 4: Store data
        var storeBlock = new ActionBlock<string>(
            async data =>
            {
                await Task.Delay(75);
                Console.WriteLine($"  [Store] Thread {Environment.CurrentManagedThreadId}: Stored {data}");
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Link the pipeline stages
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        generateBlock.LinkTo(processBlock, linkOptions);
        processBlock.LinkTo(validateBlock, linkOptions);
        validateBlock.LinkTo(storeBlock, linkOptions);

        // Feed data into pipeline
        for (int i = 1; i <= 5; i++)
        {
            await generateBlock.SendAsync(i);
        }

        generateBlock.Complete();
        await storeBlock.Completion;

        sw.Stop();
        Console.WriteLine($"\nPipeline completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task ChannelBasedPipelineExample()
    {
        Console.WriteLine("\n--- Example 2: Channel-based Pipeline ---");
        Console.WriteLine("Using System.Threading.Channels for pipeline stages\n");

        var sw = Stopwatch.StartNew();

        var channel1 = Channel.CreateUnbounded<int>();
        var channel2 = Channel.CreateUnbounded<string>();
        var channel3 = Channel.CreateUnbounded<string>();

        // Stage 1: Producer
        var producerTask = Task.Run(async () =>
        {
            for (int i = 1; i <= 5; i++)
            {
                await channel1.Writer.WriteAsync(i);
                Console.WriteLine($"  [Producer] Thread {Environment.CurrentManagedThreadId}: Produced item {i}");
                await Task.Delay(50);
            }
            channel1.Writer.Complete();
        });

        // Stage 2: Transformer
        var transformerTask = Task.Run(async () =>
        {
            await foreach (var item in channel1.Reader.ReadAllAsync())
            {
                var transformed = $"Item-{item}";
                await channel2.Writer.WriteAsync(transformed);
                Console.WriteLine($"  [Transformer] Thread {Environment.CurrentManagedThreadId}: Transformed to {transformed}");
                await Task.Delay(100);
            }
            channel2.Writer.Complete();
        });

        // Stage 3: Enricher
        var enricherTask = Task.Run(async () =>
        {
            await foreach (var item in channel2.Reader.ReadAllAsync())
            {
                var enriched = $"{item}-Enriched";
                await channel3.Writer.WriteAsync(enriched);
                Console.WriteLine($"  [Enricher] Thread {Environment.CurrentManagedThreadId}: Enriched to {enriched}");
                await Task.Delay(75);
            }
            channel3.Writer.Complete();
        });

        // Stage 4: Consumer
        var consumerTask = Task.Run(async () =>
        {
            await foreach (var item in channel3.Reader.ReadAllAsync())
            {
                Console.WriteLine($"  [Consumer] Thread {Environment.CurrentManagedThreadId}: Consumed {item}");
                await Task.Delay(50);
            }
        });

        await Task.WhenAll(producerTask, transformerTask, enricherTask, consumerTask);

        sw.Stop();
        Console.WriteLine($"\nPipeline completed in {sw.ElapsedMilliseconds}ms");
    }

    private static async Task ImageProcessingPipeline()
    {
        Console.WriteLine("\n--- Example 3: Image Processing Pipeline ---");
        Console.WriteLine("Simulating parallel image processing pipeline\n");

        var sw = Stopwatch.StartNew();

        var images = new[] { "img1.jpg", "img2.jpg", "img3.jpg", "img4.jpg", "img5.jpg" };

        // Stage 1: Load images
        var loadBlock = new TransformBlock<string, ImageData>(
            async fileName =>
            {
                await Task.Delay(100);
                Console.WriteLine($"  [Load] Loaded {fileName}");
                return new ImageData { FileName = fileName, Stage = "Loaded" };
            });

        // Stage 2: Resize images
        var resizeBlock = new TransformBlock<ImageData, ImageData>(
            async img =>
            {
                await Task.Delay(150);
                Console.WriteLine($"  [Resize] Resized {img.FileName}");
                return new ImageData { FileName = img.FileName, Stage = "Resized" };
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Stage 3: Apply filters
        var filterBlock = new TransformBlock<ImageData, ImageData>(
            async img =>
            {
                await Task.Delay(200);
                Console.WriteLine($"  [Filter] Applied filter to {img.FileName}");
                return new ImageData { FileName = img.FileName, Stage = "Filtered" };
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Stage 4: Save images
        var saveBlock = new ActionBlock<ImageData>(
            async img =>
            {
                await Task.Delay(100);
                Console.WriteLine($"  [Save] Saved {img.FileName} (Stage: {img.Stage})");
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // Link stages
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        loadBlock.LinkTo(resizeBlock, linkOptions);
        resizeBlock.LinkTo(filterBlock, linkOptions);
        filterBlock.LinkTo(saveBlock, linkOptions);

        // Process images
        foreach (var image in images)
        {
            await loadBlock.SendAsync(image);
        }

        loadBlock.Complete();
        await saveBlock.Completion;

        sw.Stop();
        Console.WriteLine($"\nImage pipeline completed in {sw.ElapsedMilliseconds}ms");
    }

    private class ImageData
    {
        public string FileName { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
    }
}