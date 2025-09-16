using System.Reflection;

namespace algs.bench;

public static class BenchFinder
{
    public static Type? FindBenchmarks(string name)
    {
        var assembly = Assembly.GetAssembly(typeof(BenchFinder));
        if (assembly == null) throw new InvalidOperationException("Benchmark assembly not found");

        var matches = assembly.GetTypes()
            .Where(t =>
                string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Name, name + "Benchmarks", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.FullName, name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => null,
            > 1 => throw new AmbiguousMatchException($"Found {matches.Count} matches for {name}"),
            _ => matches[0]
        };
    }
}
