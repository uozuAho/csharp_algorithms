// copied from https://algs4.cs.princeton.edu/15uf/QuickFindUF.java.html

using utils;

namespace algs;

public class QuickFindUf
{
    private readonly int[] _id;    // id[i] = component identifier of i
    private int _count;            // number of components

    /**
     * Initializes an empty union-find data structure with
     * {@code n} elements {@code 0} through {@code n-1}.
     * Initially, each element is in its own set.
     *
     * @param  n the number of elements
     * @throws IllegalArgumentException if {@code n < 0}
     */
    public QuickFindUf(int n)
    {
        _count = n;
        _id = new int[n];
        for (var i = 0; i < n; i++)
            _id[i] = i;
    }

    /**
     * Returns the number of sets.
     *
     * @return the number of sets (between {@code 1} and {@code n})
     */
    public int Count() {
        return _count;
    }

    /**
     * Returns the canonical element of the set containing element {@code p}.
     *
     * @param  p an element
     * @return the canonical element of the set containing {@code p}
     * @throws IllegalArgumentException unless {@code 0 <= p < n}
     */
    public int Find(int p) {
        Validate(p);
        return _id[p];
    }

    // validate that p is a valid index
    private void Validate(int p) {
        var n = _id.Length;
        if (p < 0 || p >= n) {
            throw new ArgumentOutOfRangeException(
                "index " + p + " is not between 0 and " + (n-1));
        }
    }

    /**
     * Merges the set containing element {@code p} with the set
     * containing element {@code q}.
     *
     * @param  p one element
     * @param  q the other element
     * @throws IllegalArgumentException unless
     *         both {@code 0 <= p < n} and {@code 0 <= q < n}
     */
    public void Union(int p, int q) {
        Validate(p);
        Validate(q);
        var pId = _id[p];   // needed for correctness
        var qId = _id[q];   // to reduce the number of array accesses

        // p and q are already in the same component
        if (pId == qId) return;

        for (var i = 0; i < _id.Length; i++)
            if (_id[i] == pId) _id[i] = qId;

        _count--;
    }

    /**
     * Reads an integer {@code n} and a sequence of pairs of integers
     * (between {@code 0} and {@code n-1}) from standard input, where each integer
     * in the pair represents some element;
     * if the elements are in different sets, merge the two sets
     * and print the pair to standard output.
     *
     * @param args the command-line arguments
     */
    public static void Main(string[] args) {
        var n = StdIn.ReadInt();
        var uf = new QuickFindUf(n);
        while (!StdIn.IsEmpty()) {
            var p = StdIn.ReadInt();
            var q = StdIn.ReadInt();
            if (uf.Find(p) == uf.Find(q)) continue;
            uf.Union(p, q);
            StdOut.Println(p + " " + q);
        }
        StdOut.Println(uf.Count() + " components");
    }
}
