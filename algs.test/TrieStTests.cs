using algs.Trie;
using Shouldly;

namespace algs.test;

public class TrieStTests
{
    private TrieSt<int> _trie;

    [OneTimeSetUp]
    public void Setup()
    {
        var words = new[]
        {
            "shell",
            "tortoise",
            "shelter",
            "shelley",
            "quicksand",
            "shoreline",
            "shellshock"
        };

        _trie = new TrieSt<int>();

        for (var i = 0; i < words.Length; i++)
        {
            _trie.Put(words[i], i);
        }
    }

    [Test]
    public void LongestPrefixOf()
    {
        _trie.LongestPrefixOf("shellsort").ShouldBe("shells");
        _trie.LongestPrefixOf("quicksort").ShouldBe("quicks");
    }
}
