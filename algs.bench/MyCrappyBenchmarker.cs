using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace algs.bench;

/// <summary>
/// My fast alternative to BenchmarkDotNet. For quick and approximate measurements.
/// </summary>
public static class MyCrappyBenchmarker
{
    public static void Run(Type type)
    {
        var methods = type.GetMethods(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<BenchmarkAttribute>() != null)
            .ToList();

        if (methods.Count == 0)
        {
            Console.WriteLine($"No [Benchmark] methods found in {type.Name}");
            return;
        }

        Console.WriteLine($"Running benchmarks for {type.Name}\n");

        var allMeasurements = new List<Measurements>();

        foreach (var method in methods)
        {
            foreach (var params_ in ParameterCombinations(type))
            {
                var instance = Create(type, params_);
                var measurements = RunBenchmark(method, instance);
                measurements = measurements with
                {
                    Parameters = params_.ToImmutableDictionary(
                        x => x.Key.Name,
                        x => $"{x.Value}")
                };
                allMeasurements.Add(measurements);
            }
        }

        Report(allMeasurements);
    }

    private static object? Create(Type type, Dictionary<PropertyInfo, object> paramValues)
    {
        var instance = Activator.CreateInstance(type);

        foreach (var kvp in paramValues)
        {
            kvp.Key.SetValue(instance, kvp.Value);
        }

        return instance;
    }

    private static IEnumerable<Dictionary<PropertyInfo, object>> ParameterCombinations(Type type)
    {
        var parameters = ReadParams(type);
        var props = parameters.Keys.ToList();

        var combinations = props.Aggregate(
            Seed(),
            (acc, prop) =>
                from combo in acc
                from val in parameters[prop]
                select new Dictionary<PropertyInfo, object>(combo) { [prop] = val });

        return combinations;

        IEnumerable<Dictionary<PropertyInfo, object>> Seed()
        {
            yield return new Dictionary<PropertyInfo, object>();
        }
    }

    private static Dictionary<PropertyInfo, object?[]> ReadParams(Type type)
    {
        var result = new Dictionary<PropertyInfo, object?[]>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<ParamsAttribute>();
            if (attr != null)
            {
                result[prop] = attr.Values;
            }
        }

        return result;
    }

    private static Measurements RunBenchmark(MethodInfo method, object? instance)
    {
        const int numMeasurements = 3;
        // run until last N measured rates are within this factor of each other
        const double steadyFactor = 1.3;
        const int abortAfter = 10;

        var numInvocations = new List<int>();
        var durations = new List<TimeSpan>();

        while (true)
        {
            var (invocations, time) = CountInvocations(method, instance, TimeSpan.FromMilliseconds(1));
            numInvocations.Add(invocations);
            durations.Add(time);

            if (durations.Count >= numMeasurements)
            {
                var rates = numInvocations
                    .Zip(durations)
                    .Select(x => x.First / x.Second.TotalMicroseconds)
                    .TakeLast(numMeasurements)
                    .ToList();
                if (rates.Max() / Math.Max(rates.Min(), 0.0001) <= steadyFactor)
                    break;
            }

            if (durations.Count > abortAfter)
            {
                throw new Exception($"Run time for '{method.Name}' did not stabilise after {abortAfter} runs.");
            }
        }

        var measurements = new Measurements
        {
            MethodName = method.Name,
            NumInvocations = numInvocations.TakeLast(numMeasurements).ToImmutableList(),
            Durations = durations.TakeLast(numMeasurements).ToImmutableList()
        };

        return measurements;
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

    private record Measurements
    {
        public required string MethodName { get; init; }
        public ImmutableList<int> NumInvocations { get; init; } = [];
        public ImmutableList<TimeSpan> Durations { get; init;  } = [];
        public ImmutableDictionary<string, string> Parameters { get; init; } = ImmutableDictionary<string, string>.Empty;
    }

    private static void Report(List<Measurements> measurements)
    {
        if (measurements.Count == 0) throw new ArgumentException("gimme", nameof(measurements));
        if (measurements.Any(x => x.MethodName != measurements[0].MethodName))
            throw new ArgumentException("All measurements should be for the same method", nameof(measurements));

        Console.WriteLine($"Benchmark: {measurements[0].MethodName}");

        var paramNames = measurements
            .SelectMany(x => x.Parameters.Keys)
            .Distinct()
            .ToList();

        foreach (var name in paramNames)
            Console.Write($"{name,15}");
        Console.Write($"{' ',15}per call");
        Console.WriteLine();

        foreach (var m in measurements)
        {
            foreach (var name in paramNames)
            {
                m.Parameters.TryGetValue(name, out var value);
                Console.Write($"{value,15}");
            }
            var totalInvocations = m.NumInvocations.Sum();
            var totalDuration = m.Durations.Aggregate(TimeSpan.Zero, (acc, x) => acc + x);
            var avgRatePerMs = totalInvocations / totalDuration.TotalMilliseconds;
            var perCallStr = FormatPeriod(1000 * avgRatePerMs);
            Console.Write($"{' ',15}{perCallStr}");
            Console.WriteLine();
        }
    }

    private static string FormatPeriod(double freqHz)
    {
        return freqHz switch
        {
            < 1000 => $"{1000 / freqHz:F2}ms",
            < 1_000_000 => $"{1_000_000 / freqHz:F2}µs",
            _ => $"{1_000_000_000 / freqHz:F2}ns"
        };
    }
}
