using BenchmarkDotNet.Running;

namespace algs.bench;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run -c Release algs.bench <benchmarks class name with/without 'Benchmarks'>");
            return;
        }

        var benchmarkName = args[0].ToLowerInvariant();
        var benchClass = BenchFinder.FindBenchmarks(benchmarkName);
        if (benchClass == null)
        {
            Console.WriteLine($"Could not find benchmark {benchmarkName}");
            return;
        }

        BenchmarkRunner.Run(benchClass);
    }
}
