using System.Text;
using algs.Trie;
using BenchmarkDotNet.Attributes;

namespace algs.bench.Trie;

[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
public class TernarySearchValTrieBenchmarks
{
    [Params(10, 20, 40, 80, 160, 320)]
    public int N { get; set; }

    [Benchmark]
    public void Build()
    {
        var t = new TernarySearchValTrie<int>();
        foreach (var str in RandomStrings(N, 10))
        {
            t.Put(str, 1);
        }
    }

    private static IEnumerable<string> RandomStrings(int numStrings, int maxLength)
    {
        var random = new Random();

        for (var i = 0; i < numStrings; i++)
        {
            var length = random.Next(maxLength - 1) + 1;
            var sb = new StringBuilder(length);
            for (var j = 0; j < length; j++)
            {
                sb.Append((char)random.Next('a', 'z'));
            }
            yield return sb.ToString();
        }
    }
}
