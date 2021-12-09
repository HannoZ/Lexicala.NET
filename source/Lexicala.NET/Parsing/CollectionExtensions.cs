using System;
using System.Collections.Generic;

namespace Lexicala.NET.Parsing
{
    internal static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection)
        {
            foreach(var item in collection)
            {
                source.Add(item);
            }
        }
    }
}
