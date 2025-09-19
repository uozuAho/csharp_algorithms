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
        var methodsToBenchmark = type.GetMethods(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.Static)
            .Where(m => m.GetCustomAttribute<BenchmarkAttribute>() != null)
            .ToList();

        if (methodsToBenchmark.Count == 0)
        {
            Console.WriteLine($"No [Benchmark] methods found in {type.Name}");
            return;
        }

        var globalSetup = type
            .GetMethods(BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.Static)
            .SingleOrDefault(m => m.GetCustomAttribute<GlobalSetupAttribute>() != null);

        foreach (var method in methodsToBenchmark)
        {
            var allMeasurements = new List<SingleConfigMeasurements>();

            foreach (var paramValues in ParameterCombinations(type))
            {
                var measurements = RunBenchmark(type, paramValues, method, globalSetup);
                allMeasurements.Add(measurements);
            }

            var mb = MethodBenchmarks.From(type, method.Name, allMeasurements);
            Report(mb);
        }
    }

    private static SingleConfigMeasurements RunBenchmark(
        Type type,
        Dictionary<PropertyInfo, object> paramValues,
        MethodInfo methodToBench,
        MethodInfo? globalSetup
    )
    {
        const int numMeasurements = 5;
        // run until last N measured rates are within this factor of each other
        const double steadyFactor = 1.6;
        const int abortAfter = 40;

        var numInvocations = new List<int>();
        var durations = new List<TimeSpan>();
        var allocatedBytes = new List<long>();

        var instance = Create(type, paramValues);
        if (globalSetup != null)
        {
            globalSetup.Invoke(instance, null);
        }

        while (true)
        {
            var (invocations, time, allocated) = RunForTime(
                methodToBench, instance, TimeSpan.FromMilliseconds(1));
            numInvocations.Add(invocations);
            durations.Add(time);
            allocatedBytes.Add(allocated);

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
                throw new Exception($"Run time for '{methodToBench.Name}' did not stabilise after {abortAfter} runs.");
            }
        }

        return new SingleConfigMeasurements
        {
            ParameterValues = paramValues.ToImmutableDictionary(
                x => x.Key.Name,
                x => $"{x.Value}"),
            NumInvocations = numInvocations.TakeLast(numMeasurements).ToImmutableList(),
            Durations = durations.TakeLast(numMeasurements).ToImmutableList(),
            AllocatedBytes = allocatedBytes.TakeLast(numMeasurements).ToImmutableList(),
        };
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

    private static (int invocations, TimeSpan actualDuration, long totalBytesAllocated)
        RunForTime(MethodInfo method, object? instance, TimeSpan duration)
    {
        var invocations = 0;
        var totalBytesAllocated = 0L;

        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < duration)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            method.Invoke(instance, null);
            var after = GC.GetAllocatedBytesForCurrentThread();
            totalBytesAllocated += after - before;
            invocations++;
        }
        sw.Stop();

        return (invocations, sw.Elapsed, totalBytesAllocated);
    }

    private record MethodBenchmarks
    {
        public required Type BenchmarksType { get; init; }
        public required string MethodName { get; init; }
        public ImmutableList<string> ParameterNames { get; private init; } = [];
        public ImmutableList<SingleConfigMeasurements> Measurements { get; private init; } = [];

        public static MethodBenchmarks From(
            Type benchmarksType,
            string methodName,
            List<SingleConfigMeasurements> measurements
        )
        {
            var paramNames = benchmarksType.GetProperties()
                .Where(x => x.GetCustomAttribute<ParamsAttribute>() != null)
                .Select(x => x.Name)
                .ToImmutableList();

            return new MethodBenchmarks()
            {
                BenchmarksType = benchmarksType,
                MethodName = methodName,
                ParameterNames = paramNames,
                Measurements = measurements.ToImmutableList(),
            };
        }
    }

    /// <summary>
    /// Measurements for the given set of parameters. Bad name :(
    /// </summary>
    private record SingleConfigMeasurements
    {
        public ImmutableDictionary<string, string> ParameterValues { get; init; } =
            ImmutableDictionary<string, string>.Empty;
        public ImmutableList<int> NumInvocations { get; init; } = [];
        public ImmutableList<TimeSpan> Durations { get; init;  } = [];
        public ImmutableList<long> AllocatedBytes { get; init; } = [];
    }

    private static void Report(MethodBenchmarks benchmarks)
    {
        if (benchmarks.Measurements.Count == 0)
            throw new ArgumentException("gimme", nameof(benchmarks));

        Console.WriteLine($"Benchmark: {benchmarks.BenchmarksType.Name}.{benchmarks.MethodName}");

        foreach (var name in benchmarks.ParameterNames)
            Console.Write($"{name,15}");
        Console.Write($"{"per call",15}");
        Console.Write($"{"allocated",15}");
        Console.WriteLine();

        foreach (var m in benchmarks.Measurements)
        {
            foreach (var name in benchmarks.ParameterNames)
            {
                var value = m.ParameterValues[name];
                Console.Write($"{value,15}");
            }

            var totalInvocations = m.NumInvocations.Sum();
            var totalDuration = m.Durations.Aggregate(TimeSpan.Zero, (acc, x) => acc + x);
            var avgRatePerMs = totalInvocations / totalDuration.TotalMilliseconds;
            var perCallStr = FormatPeriod(1000 * avgRatePerMs);
            Console.Write($"{perCallStr,15}");

            var allocatedPerCall = (long)m.AllocatedBytes
                .Zip(m.NumInvocations)
                .Select(x => x.First / x.Second)
                .Average();
            Console.WriteLine($"{FormatBytes(allocatedPerCall),15}");
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

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        var order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        // Show 1 decimal place for larger values, but no decimals for bytes
        return order == 0
            ? $"{len:0} {sizes[order]}"
            : $"{len:0.##} {sizes[order]}";
    }
}
