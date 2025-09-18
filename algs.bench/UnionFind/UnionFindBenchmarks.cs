using BenchmarkDotNet.Attributes;

namespace algs.bench.UnionFind;

[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
public class UnionFindBenchmarks
{
    private QuickFindUf _uf = new(1);

    [Params(10, 100, 1000)]
    public int N { get; set; }

    [GlobalSetup]
    public void QuickFindUf()
    {
        _uf = new QuickFindUf(N);
    }

    [Benchmark]
    public void Union()
    {
        // todo: is this a good test of union?
        for (var i = 0; i < N - 1; i++)
        {
            _uf.Union(i, i + 1);
        }
    }
}
