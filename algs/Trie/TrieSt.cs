// copied from https://algs4.cs.princeton.edu/52trie/TrieST.java.html

namespace algs.Trie;

/// <summary>
/// Trie symbol table
/// </summary>
public class TrieSt<T>
{
    private const int BranchFactor = 256;

    private Node? _root = null;
    private int _size = 0;

    private class Node
    {
        public T? Value { get; set; }
        public Node[] Children { get; set; } = new Node[BranchFactor];
    }

    public TrieSt()
    {
    }

    public T? Get(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var x = Get(_root, key, 0);
        return x == null ? default : x.Value;
    }

    private static Node? Get(Node? x, string key, int d)
    {
        while (true)
        {
            if (x == null) return null;
            if (d == key.Length) return x;
            var c = key[d];
            x = x.Children[c];
            d += 1;
        }
    }

    private Node Put(Node? current, string key, T value, int d)
    {
        current ??= new Node();

        if (d == key.Length)
        {
            if (current.Value == null) _size++;
            current.Value = value;
            return current;
        }

        var c = key[d];
        current.Children[c] = Put(current.Children[c], key, value, d + 1);

        return current;
    }
}
