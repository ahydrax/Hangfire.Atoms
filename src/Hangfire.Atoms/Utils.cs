using System.Collections.Generic;

namespace Hangfire.Atoms
{
    internal static class Utils
    {
        public static V GetByKey<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            dictionary.TryGetValue(key, out var val);
            return val;
        }
    }
}
