namespace GodotInterfaceExport.Utilities;
using System.Collections.Generic;

internal static class ObjectUtils
{
    public static void AddOrMerge<TKey, TValue>(
        this Dictionary<TKey, HashSet<TValue>> dictionary,
        TKey key,
        HashSet<TValue> set
    ) where TKey : notnull
    {
        if (set.Count == 0)
            return;

        if (dictionary.ContainsKey(key))
        {
            dictionary[key].UnionWith(set);
        }
        else
        {
            dictionary[key] = set;
        }
    }
}
