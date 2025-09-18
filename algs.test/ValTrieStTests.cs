using algs.Trie;
using Shouldly;

namespace algs.test;

[TestFixture("valtrie")]
[TestFixture("tst")]
public class ValTrieStTests
{
    private readonly string _trieType;
    private IValTrieSt<int> _trie;

    public ValTrieStTests(string trieType)
    {
        _trieType = trieType;
        _trie = CreateNewTrie(trieType);
    }

    [SetUp]
    public void Setup()
    {
        _trie = CreateNewTrie(_trieType);
    }

    [TestCase("shellsort", "shell")]
    [TestCase("quicksort", null)]
    public void LongestPrefixOf(string word, string prefix)
    {
        AddWords([
            "shell",
            "shelley",
            "quicksand"
        ]);

        _trie.LongestPrefixOf(word).ShouldBe(prefix);
    }

    [TestCase("shor", "shoreline,shorty")]
    public void KeysWithPrefix(string prefix, string expected)
    {
        AddWords([
            "shoreline",
            "shorty",
            "babababa"
        ]);

        var expectedKeys = expected.Split(",");
        _trie.KeysWithPrefix(prefix).ShouldBe(expectedKeys);
    }

    private static IValTrieSt<int> CreateNewTrie(string type)
    {
        return type switch
        {
            "valtrie" => new ValTrieSt<int>(),
            "tst" => new TernarySearchValTrie<int>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private void AddWords(string[] words)
    {
        for (var i = 0; i < words.Length; i++)
        {
            _trie.Put(words[i], i);
        }
    }
}
