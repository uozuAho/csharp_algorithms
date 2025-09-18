namespace algs.Trie;

/// <summary>
/// Trie symbol table, for value types. I have to constrain T to either
/// a struct/class otherwise T? doesn't work properly in Node. Eg. if I
/// don't constrain T, then T? Value = default = 0 (instead of null)
/// when T = int. Dunno why.
/// </summary>
public interface IValTrieSt<T> where T : struct
{
    void Put(string key, T value);
    void Delete(string key);

    T? Get(string key);
    IEnumerable<string> Keys();
    IEnumerable<string> KeysWithPrefix(string prefix);
    string? LongestPrefixOf(string query);
}
