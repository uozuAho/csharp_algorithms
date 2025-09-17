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

        var benchmarkName = args.SingleOrDefault(x => x != "--help" && x != "--bnet");
        if (benchmarkName == null)
        {
            Console.WriteLine("Please specify a benchmark name.");
            return;
        }
        var benchClass = BenchFinder.FindBenchmarks(benchmarkName);
        if (benchClass == null)
        {
            Console.WriteLine($"Could not find benchmark {benchmarkName}");
            return;
        }

        if (args.Contains("--bnet"))
            BenchmarkRunner.Run(benchClass);
        else
            MyCrappyBenchmarker.Run(benchClass);
    }
}
