using System.Collections.Immutable;
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
                // Report2(measurements);
            }
        }

        Report3(allMeasurements);
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

    private static void Report(Measurements measurements)
    {
        Console.WriteLine($"Benchmark: {measurements.MethodName}");
        Console.WriteLine("Parameters:");
        foreach (var kvp in measurements.Parameters)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        Console.WriteLine($"    {"Rate (/ms)",-14} {"Per Call",-15}");
        for (var i = 0; i < measurements.NumInvocations.Count; i++)
        {
            var rate = measurements.NumInvocations[i] / measurements.Durations[i].TotalMilliseconds;
            var perCall = TimePerCall(measurements, i);
            var perCallStr = FormatTime(perCall);
            Console.WriteLine($"  {rate,12:#.##} {perCallStr,15}");
        }
    }

    private static void Report2(Measurements measurements)
    {
        Console.WriteLine($"Benchmark: {measurements.MethodName}");
        Console.WriteLine("Parameters:");
        foreach (var kvp in measurements.Parameters)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        var avgRatePerMs = measurements.NumInvocations.Sum()
                           / measurements.Durations.Sum(x => x.TotalMilliseconds);

        Console.WriteLine($"{avgRatePerMs} calls per ms");
        Console.WriteLine($"{FormatPeriod(1000 * avgRatePerMs)} per call");
    }

    private static void Report3(List<Measurements> measurements)
    {
        if (measurements.Count == 0) throw new ArgumentException("gimme", nameof(measurements));
        if (measurements.Any(x => x.MethodName != measurements[0].MethodName))
            throw new ArgumentException("All measurements should be for the same method", nameof(measurements));

        Console.WriteLine($"Benchmark: {measurements[0].MethodName}");

        // Get all unique parameter names
        var paramNames = measurements
            .SelectMany(x => x.Parameters.Keys)
            .Distinct()
            .ToList();

        // Print header
        foreach (var name in paramNames)
            Console.Write($"{name,15}");
        Console.Write($"{' ',15}per call");
        Console.WriteLine();

        // Print each row
        foreach (var m in measurements)
        {
            foreach (var name in paramNames)
            {
                m.Parameters.TryGetValue(name, out var value);
                Console.Write($"{value,15}");
            }
            // Calculate per call as in Report2
            var totalInvocations = m.NumInvocations.Sum();
            var totalDuration = m.Durations.Aggregate(TimeSpan.Zero, (acc, x) => acc + x);
            var avgRatePerMs = totalInvocations / totalDuration.TotalMilliseconds;
            var perCallStr = FormatPeriod(1000 * avgRatePerMs);
            Console.Write($"{' ',15}{perCallStr}");
            Console.WriteLine();
        }
    }

    private static string FormatTime(TimeSpan ts)
    {
        if (ts.TotalMilliseconds >= 1)
            return $"{ts.TotalMilliseconds:F2}ms";
        if (ts.TotalMicroseconds >= 1)
            return $"{ts.TotalMicroseconds:F2}µs";
        return $"{ts.TotalNanoseconds:F2}ns";
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

    // TODO: use something with more resolution than timespan. double nanoseconds?
    private static TimeSpan TimePerCall(Measurements measurements, int i)
    {
        var duration = measurements.Durations[i];
        var invocations = measurements.NumInvocations[i];
        var perCall = duration / invocations;
        return perCall;
    }
}
