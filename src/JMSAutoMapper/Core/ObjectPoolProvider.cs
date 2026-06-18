using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JMSAutoMapper.Core
{
    /// <summary>
    /// Provedor central de pooling de objetos para reduzir alocação de GC.
    /// </summary>
    public static class ObjectPoolProvider
    {
        private static readonly ConcurrentQueue<Dictionary<object, object>> _dictionaryPool = new();
        private static readonly ConcurrentQueue<HashSet<object>> _hashSetPool = new();

        public static Dictionary<object, object> GetDictionary()
        {
            if (_dictionaryPool.TryDequeue(out var dict)) return dict;
            return new Dictionary<object, object>(Reflection.ReferenceEqualityComparer.Instance);
        }

        public static void ReturnDictionary(Dictionary<object, object> dict)
        {
            dict.Clear();
            _dictionaryPool.Enqueue(dict);
        }

        public static HashSet<object> GetHashSet()
        {
            if (_hashSetPool.TryDequeue(out var set)) return set;
            return new HashSet<object>(Reflection.ReferenceEqualityComparer.Instance);
        }

        public static void ReturnHashSet(HashSet<object> set)
        {
            set.Clear();
            _hashSetPool.Enqueue(set);
        }
    }
}