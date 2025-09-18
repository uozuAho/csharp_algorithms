using BenchmarkDotNet.Running;

namespace algs.bench;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine(
                """
                Usage: dotnet run -c Release algs.bench <benchmarks class name with/without 'Benchmarks'>

                Options:
                --help:        show this message and exit
                --bnet:        use BenchmarkDotNet instead of my crappy benchmarker
                """);
        }

        var useBenchDotNet = args.Contains("--bnet");
        var benchmarkNames = args.Where(x => x != "--help" && x != "--bnet").ToList();
        if (benchmarkNames.Count == 0)
        {
            Console.WriteLine("Please specify a benchmark name.");
            return;
        }

        foreach (var benchmarkName in benchmarkNames)
        {
            RunBenchmarks(benchmarkName, useBenchDotNet);
        }
    }

    private static void RunBenchmarks(string benchmarkName, bool useBenchDotNet)
    {
        var benchClass = BenchFinder.FindBenchmarks(benchmarkName);
        if (benchClass == null)
        {
            Console.WriteLine($"Could not find benchmark {benchmarkName}");
            return;
        }

        if (useBenchDotNet)
            BenchmarkRunner.Run(benchClass);
        else
            MyCrappyBenchmarker.Run(benchClass);
    }
}
