using JMSAutoMapper.Abstractions;
using JMSAutoMapper.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JMSAutoMapper.Collections
{
    /// <summary>
    /// Mapeador especializado para Arrays.
    /// </summary>
    internal static class ArrayMapper
    {
        public static object Map(IEnumerable source, Type itemType, IMapper mapper, Dictionary<object, object> mappedObjects)
        {
            var list = ListMapper.MapToList(source, itemType, mapper, mappedObjects);
            var array = Array.CreateInstance(itemType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }
    }
}