using ParallelismDemo.Examples;

namespace ParallelismDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("PARALLELISM DEMO APPLICATION");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await DataParallelismExamples.RunAllExamples();
                        break;
                    case "2":
                        await TaskParallelismExamples.RunAllExamples();
                        break;
                    case "3":
                        await PipelineParallelismExamples.RunAllExamples();
                        break;
                    case "4":
                        await LevelsOfParallelismExamples.RunAllExamples();
                        break;
                    case "5":
                        await ParallelPatterns.RunAllExamples();
                        break;
                    case "6":
                        PerformanceComparison.RunComparison();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please select a valid option.");
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("\nPress Enter to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine("\n" + "=".PadRight(80, '='));
        Console.WriteLine("SELECT A PARALLELISM CONCEPT TO EXPLORE:");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();
        Console.WriteLine("TYPES OF PARALLELISM:");
        Console.WriteLine("  1. Data Parallelism");
        Console.WriteLine("  2. Task Parallelism");
        Console.WriteLine("  3. Pipeline Parallelism");
        Console.WriteLine();
        Console.WriteLine("LEVELS OF PARALLELISM:");
        Console.WriteLine("  4. Bit-Level, Instruction-Level, Thread-Level, Process-Level");
        Console.WriteLine();
        Console.WriteLine("ADVANVED PATTERNS:");
        Console.WriteLine("  5. Parallel Patterns (Producer-Consumer), Fork-Join, MapReduce");
        Console.WriteLine("  6. Performance Comparision (Sequential vs Parallel");
        Console.WriteLine();
        Console.WriteLine("  0. Exit");
        Console.WriteLine();
        Console.WriteLine("Enter your choice: ");
    }


}