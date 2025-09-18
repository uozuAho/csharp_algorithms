using System.Text;

namespace algs.Trie;

public class TernarySearchValTrie<T> : IValTrieSt<T> where T : struct
{
    public int Size { get; private set; }

    private Node? _root;

    public TernarySearchValTrie()
    {
    }

    private class Node
    {
        public char c;
        public T? Value { get; set; }

        public Node? Left { get; set; }
        public Node? Mid { get; set; }
        public Node? Right { get; set; }
    }

    public bool Contains(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Get(key) != null;
    }

    public void Delete(string key)
    {
        Put(_root, key, null, 0);
    }

    public T? Get(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var x = Get(_root, key, 0);
        return x?.Value;
    }

    public void Put(string key, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!Contains(key)) Size++;
        _root = Put(_root, key, value, 0);
    }

    private static Node Put(Node? x, string key, T? value, int depth)
    {
        var c = key[depth];
        x ??= new Node { c = c };
        if      (c < x.c)                 x.Left  = Put(x.Left,  key, value, depth);
        else if (c > x.c)                 x.Right = Put(x.Right, key, value, depth);
        else if (depth < key.Length - 1)  x.Mid   = Put(x.Mid,   key, value, depth + 1);
        else                              x.Value = value;
        return x;
    }

    private static Node? Get(Node? x, string key, int depth)
    {
        if (x == null) return null;
        ArgumentException.ThrowIfNullOrEmpty(key);
        var c = key[depth];
        if (c < x.c)                   return Get(x.Left,  key, depth);
        if (c > x.c)                   return Get(x.Right, key, depth);
        if (depth < key.Length - 1)    return Get(x.Mid,   key, depth + 1);
        return x;
    }

    public IEnumerable<string> Keys()
    {
        var queue = new Queue<string>();
        Collect(_root, new StringBuilder(), queue);
        return queue;
    }

    private static void Collect(Node? x, StringBuilder prefix, Queue<string> queue)
    {
        while (true)
        {
            if (x == null) return;
            Collect(x.Left, prefix, queue);
            if (x.Value != null) queue.Enqueue(prefix.ToString() + x.c);
            Collect(x.Mid, prefix.Append(x.c), queue);
            prefix.Remove(prefix.Length - 1, 1);
            x = x.Right;
        }
    }

    public IEnumerable<string> KeysWithPrefix(string prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        var queue = new Queue<string>();
        var x = Get(_root, prefix, 0);
        if (x == null) return queue;
        if (x.Value != null) queue.Enqueue(prefix);
        Collect(x.Mid, new StringBuilder(prefix), queue);
        return queue;
    }

    public string? LongestPrefixOf(string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);
        var length = 0;
        var node = _root;
        var i = 0;
        while (node != null && i < query.Length)
        {
            var c = query[i];
            if      (c < node.c) node = node.Left;
            else if (c > node.c) node = node.Right;
            else
            {
                i++;
                if (node.Value != null) length = i;
                node = node.Mid;
            }
        }
        return length == 0 ? null : query[..length];
    }

    public string DebugString()
    {
        var sb = new StringBuilder();
        DebugStringRec(_root, sb, 0);
        return sb.ToString();
    }

    private static void DebugStringRec(Node? x, StringBuilder sb, int indent)
    {
        if (x == null) return;
        sb.Append(' ', indent);
        sb.Append(x.c);
        if (x.Value != null)
            sb.Append(": ").Append(x.Value);
        sb.AppendLine();
        DebugStringRec(x.Left, sb, indent + 2);
        DebugStringRec(x.Mid, sb, indent + 2);
        DebugStringRec(x.Right, sb, indent + 2);
    }
}
