using algs.Trie;
using Shouldly;

namespace algs.test;

[TestFixture("A")]
public class ValTrieStTests
{
    public ValTrieStTests()
    {

    }

    private static ValTrieSt<int> IntTrieWith(string[] words)
    {
        var t = new ValTrieSt<int>();

        for (var i = 0; i < words.Length; i++)
        {
            t.Put(words[i], i);
        }

        return t;
    }

    [Test]
    public void DebugString()
    {
        var t = new ValTrieSt<int>();
        t.Put("alf", 1);
        t.Put("abc", 2);

        t.DebugString().ShouldBe(
            """
            a
              b
                c: 2
              l
                f: 1
            """ + Environment.NewLine);
    }

    [TestCase("shellsort", "shell")]
    [TestCase("quicksort", null)]
    public void LongestPrefixOf(string word, string prefix)
    {
        var trie = IntTrieWith([
            "shell",
            "shelley",
            "quicksand"
        ]);

        trie.LongestPrefixOf(word).ShouldBe(prefix);
    }

    [TestCase("shor", "shoreline,shorty")]
    public void KeysWithPrefix(string prefix, string expected)
    {
        var trie = IntTrieWith([
            "shoreline",
            "shorty",
            "babababa"
        ]);

        var expectedKeys = expected.Split(",");
        trie.KeysWithPrefix(prefix).ShouldBe(expectedKeys);
    }
}
