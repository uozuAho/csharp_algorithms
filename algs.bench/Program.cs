using algs.bench.UnionFind;
using BenchmarkDotNet.Running;

namespace algs.bench;

public class Program
{
    public static void Main(string[] args)
        => BenchmarkRunner.Run<UnionFindBenchmarks>();
}
