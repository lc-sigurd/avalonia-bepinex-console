using System.Collections.Generic;

namespace Sigurd.AvaloniaBepInExConsole.App.Collections;

public class MultiLookup<TKey1, TKey2> where TKey1 : notnull where TKey2 : notnull
{
    private readonly Dictionary<TKey1, HashSet<TKey2>> _lookup = new();

    public bool Add(TKey1 key1, TKey2 key2)
    {
        if (_lookup.TryGetValue(key1, out var set))
            return set.Add(key2);

        set = new HashSet<TKey2> { key2 };
        _lookup[key1] = set;
        return true;
    }

    public bool Remove(TKey1 key1, TKey2 key2)
    {
        if (!_lookup.TryGetValue(key1, out var set))
            return false;
        var result = set.Remove(key2);
        if (!result)
            return false;
        if (set.Count == 0) _lookup.Remove(key1);
        return true;
    }

    public IEnumerable<TKey2> this[TKey1 key] {
        get {
            if (_lookup.TryGetValue(key, out var set)) return set;
            return [];
        }
    }
}
