using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace algs.bench;

/// <summary>
/// My fast alternative to BenchmarkDotNet. For quick and approximate measurements.
/// </summary>
public class MyCrappyBenchmarker
{
    public static void Run(Type type, int iterations = 20)
    {
        var methods = type.GetMethods(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<BenchmarkAttribute>() != null)
            .ToList();

        if (!methods.Any())
        {
            Console.WriteLine($"No [Benchmark] methods found in {type.Name}");
            return;
        }

        object? instance = null;
        if (!methods.All(m => m.IsStatic))
            instance = Activator.CreateInstance(type);

        Console.WriteLine($"Running benchmarks for {type.Name}\n");

        foreach (var method in methods)
        {
            Console.WriteLine($"Benchmark: {method.Name}");

            // Warmup (JIT)
            method.Invoke(instance, null);

            var times = new List<double>(iterations);

            for (int i = 0; i < iterations; i++)
            {
                var sw = Stopwatch.StartNew();
                method.Invoke(instance, null);
                sw.Stop();

                times.Add(sw.Elapsed.TotalMilliseconds);
            }

            PrintStats(times);
            Console.WriteLine();
        }
    }

    private static void PrintStats(List<double> times)
    {
        double avg = times.Average();
        double min = times.Min();
        double max = times.Max();
        double stddev = Math.Sqrt(times.Select(t => Math.Pow(t - avg, 2)).Average());

        Console.WriteLine($"  N = {times.Count}");
        Console.WriteLine($"  Mean   = {avg:F4} ms");
        Console.WriteLine($"  StdDev = {stddev:F4} ms");
        Console.WriteLine($"  Min    = {min:F4} ms");
        Console.WriteLine($"  Max    = {max:F4} ms");
    }
}
