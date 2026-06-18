using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JMSAutoMapper.Internals
{
    /// <summary>
    /// Extensões para manipulação de dicionários concorrentes, facilitando a mesclagem de configurações de mapeamento.
    /// </summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Mescla os elementos de um dicionário de origem no dicionário de destino usando AddOrUpdate.
        /// </summary>
        public static void Merge<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> target, ConcurrentDictionary<TKey, TValue> source)
            where TKey : notnull
        {
            foreach (var kvp in source)
            {
                target.AddOrUpdate(kvp.Key, kvp.Value, (_, __) => kvp.Value);
            }
        }

        /// <summary>
        /// Mescla dicionários aninhados (ConcurrentDictionary de ConcurrentDictionary), garantindo a integridade das sub-coleções.
        /// </summary>
        public static void MergeNested<TKey, TSubKey, TValue>(
            this ConcurrentDictionary<TKey, ConcurrentDictionary<TSubKey, TValue>> target,
            ConcurrentDictionary<TKey, ConcurrentDictionary<TSubKey, TValue>> source)
            where TKey : notnull
            where TSubKey : notnull
        {
            foreach (var kvp in source)
            {
                var targetSubDict = target.GetOrAdd(kvp.Key, _ => new ConcurrentDictionary<TSubKey, TValue>());
                foreach (var subKvp in kvp.Value)
                {
                    targetSubDict.AddOrUpdate(subKvp.Key, subKvp.Value, (_, __) => subKvp.Value);
                }
            }
        }

        /// <summary>
        /// Mescla dicionários de listas, concatenando os itens nas listas correspondentes com proteção de lock para as listas.
        /// </summary>
        public static void MergeLists<TKey, TValue>(this ConcurrentDictionary<TKey, List<TValue>> target, ConcurrentDictionary<TKey, List<TValue>> source)
            where TKey : notnull
        {
            foreach (var kvp in source)
            {
                var targetList = target.GetOrAdd(kvp.Key, _ => new List<TValue>());
                lock (targetList)
                {
                    targetList.AddRange(kvp.Value);
                }
            }
        }
    }
}