using System.Text;
using algs.Trie;
using BenchmarkDotNet.Attributes;

namespace algs.bench.Trie;

[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
public class TernarySearchValTrieBenchmarks
{
    [Params(10)]
    public int StringLen { get; set; }

    [Params("random", "sorted asc", "sorted desc")]
    public string StringOrder { get; set; } = "";

    [Params(10, 20, 40, 80, 160, 320)]
    public int NumStrings { get; set; }

    private string[] _randomStrings = [];
    private string[] _randomStringsSorted = [];
    private string[] _randomStringsSortedReversed = [];

    [GlobalSetup]
    public void Setup()
    {
        _randomStrings = RandomStrings(NumStrings, StringLen).ToArray();
        _randomStringsSorted = _randomStrings.OrderBy(x => x).ToArray();
        _randomStringsSortedReversed = _randomStrings.OrderByDescending(x => x).ToArray();
    }

    [Benchmark]
    public void Build()
    {
        var strings = StringOrder switch
        {
            "random" => _randomStrings,
            "sorted asc" => _randomStringsSorted,
            "sorted desc" => _randomStringsSortedReversed,
            _ => throw new ArgumentOutOfRangeException()
        };

        var t = new TernarySearchValTrie<int>();
        foreach (var str in strings)
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
