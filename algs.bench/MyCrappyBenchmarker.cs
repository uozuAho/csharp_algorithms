using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace algs.bench;

/// <summary>
/// My fast alternative to BenchmarkDotNet. For quick and approximate measurements.
/// </summary>
public class MyCrappyBenchmarker
{
    public static void Run(Type type)
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
            RunBenchmark(method, instance);
        }
    }

    private static void RunBenchmark(MethodInfo method, object? instance)
    {
        var measurements = new Measurements { MethodName = method.Name };

        for (var i = 0; i < 3; i++)
        {
            var (invocations, time) = CountInvocations(method, instance, TimeSpan.FromMilliseconds(1));

            measurements.NumInvocations.Add(invocations);
            measurements.Durations.Add(time);
        }

        Report(measurements);
    }

    private static (int, TimeSpan) CountInvocations(
        MethodInfo method, object? instance, TimeSpan duration)
    {
        var invocations = 0;

        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < duration)
        {
            method.Invoke(instance, null);
            invocations++;
        }
        sw.Stop();

        return (invocations, sw.Elapsed);
    }

    private class Measurements
    {
        public required string MethodName { get; init; }
        public List<int> NumInvocations { get; init; } = [];
        public List<TimeSpan> Durations { get; init; } = [];
    }

    private static void Report(Measurements measurements)
    {
        Console.WriteLine(measurements.MethodName);
        for (var i = 0; i < measurements.NumInvocations.Count; i++)
        {
            var rate = measurements.NumInvocations[i] / measurements.Durations[i].TotalMilliseconds;
            var perCall = measurements.Durations[i].TotalMilliseconds / measurements.NumInvocations[i];
            Console.WriteLine($"  {rate:#.##}/ms    {perCall:##.00000}ms per call");
        }
    }
}
