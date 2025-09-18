using BenchmarkDotNet.Attributes;

namespace algs.bench.UnionFind;

[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
public class QuickFindUf_Find
{
    private QuickFindUf _uf = new(1);

    [Params(10, 100, 1000)]
    public int N { get; set; }

    [GlobalSetup]
    public void QuickFindUf()
    {
        _uf = new QuickFindUf(N);
        for (var i = 0; i < N - 1; i++)
        {
            _uf.Union(i, i + 1);
        }
    }

    [Benchmark]
    public void Find()
    {
        // todo: is this a good test of find??
        _uf.Find(N / 2);
    }
}
