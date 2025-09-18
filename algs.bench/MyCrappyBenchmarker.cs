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
                Report(measurements);
            }
        }
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
        var measurements = new Measurements { MethodName = method.Name };

        for (var i = 0; i < 3; i++)
        {
            // todo: support methods that run for longer than 1 ms
            var (invocations, time) = CountInvocations(method, instance, TimeSpan.FromMilliseconds(1));

            measurements = measurements with
            {
                NumInvocations = measurements.NumInvocations.Add(invocations),
                Durations = measurements.Durations.Add(time)
            };
        }

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

    private static string FormatTime(TimeSpan ts)
    {
        if (ts.TotalMilliseconds >= 1)
            return $"{ts.TotalMilliseconds:F2}ms";
        if (ts.TotalMicroseconds >= 1)
            return $"{ts.TotalMicroseconds:F2}µs";
        return $"{ts.TotalNanoseconds:F2}ns";
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
