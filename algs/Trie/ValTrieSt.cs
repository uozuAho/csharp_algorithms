// copied from https://algs4.cs.princeton.edu/52trie/TrieST.java.html

using System.Text;

namespace algs.Trie;

public class ValTrieSt<T> : IValTrieSt<T> where T : struct
{
    private const int ChildrenPerNode = 256;

    private Node? _root;
    private int _size;

    private class Node
    {
        public T? Value { get; set; }

        public Node?[] Children { get; set; } = new Node[ChildrenPerNode];
    }

    public ValTrieSt()
    {
    }

    public T? Get(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var x = Get(_root, key, 0);
        return x?.Value;
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

    public void Put(string key, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _root = Put(_root, key, value, 0);
    }

    private Node Put(Node? current, string key, T? value, int depth)
    {
        current ??= new Node();

        if (depth == key.Length)
        {
            if (current.Value == null) _size++;
            current.Value = value;
            return current;
        }

        var c = key[depth];
        current.Children[c] = Put(current.Children[c], key, value, depth + 1);

        return current;
    }

    public IEnumerable<string> Keys() => KeysWithPrefix("");

    public IEnumerable<string> KeysWithPrefix(string prefix)
    {
        var results = new Queue<string>();
        var x = Get(_root, prefix, 0);
        Collect(x, new StringBuilder(prefix), results);
        return results;
    }

    private void Collect(Node? x, StringBuilder prefix, Queue<string> results)
    {
        if (x == null) return;
        if (x.Value != null) results.Enqueue(prefix.ToString());
        for (var c = 0; c < ChildrenPerNode; c++)
        {
            prefix.Append((char)c);
            Collect(x.Children[c], prefix, results);
            prefix.Remove(prefix.Length - 1, 1);
        }
    }

    public void Delete(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _root = Delete(_root, key, 0);
    }

    private Node? Delete(Node? x, string key, int depth)
    {
        if (x == null) return null;

        if (depth == key.Length)
        {
            if (x.Value != null) _size--;
            x.Value = null;
        }
        else
        {
            var c = key[depth];
            x.Children[c] = Delete(x.Children[c], key, depth + 1);
        }

        // remove subtrie rooted at x if it is completely empty
        if (x.Value != null) return x;
        for (var c = 0; c < ChildrenPerNode; c++)
        {
            if (x.Children[c] != null)
                return x;
        }
        return null;
    }

    /**
     * Returns the string in the symbol table that is the longest prefix of {@code query},
     * or {@code null}, if no such string.
     * @param query the query string
     * @return the string in the symbol table that is the longest prefix of {@code query},
     *     or {@code null} if no such string
     * @throws IllegalArgumentException if {@code query} is {@code null}
     */
    public string? LongestPrefixOf(string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);
        var length = LongestPrefixOf(_root, query, 0, -1);
        return length == -1
            ? null
            : query[..length];
    }

    // returns the length of the longest string key in the subtrie
    // rooted at x that is a prefix of the query string,
    // assuming the first d character match and we have already
    // found a prefix match of given length (-1 if no such match)
    private static int LongestPrefixOf(Node? current, string query, int depth, int length)
    {
        while (true)
        {
            if (current == null) return length;
            if (current.Value != null) length = depth;
            if (depth == query.Length) return length;
            var c = query[depth];
            current = current.Children[c];
            depth += 1;
        }
    }

    public string DebugString()
    {
        var sb = new StringBuilder();
        DebugStringRec(_root, sb, 0);
        return sb.ToString();
    }

    private void DebugStringRec(Node? x, StringBuilder sb, int indent)
    {
        if (x == null) return;
        for (var c = 0; c < ChildrenPerNode; c++)
        {
            var child = x.Children[c];
            if (child != null)
            {
                sb.Append(' ', indent);
                sb.Append((char)c);
                if (child.Value != null)
                    sb.Append(": ").Append(child.Value);
                sb.AppendLine();
                DebugStringRec(child, sb, indent + 2);
            }
        }
    }
}
