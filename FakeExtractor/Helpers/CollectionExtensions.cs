using System.Collections.Generic;

namespace FakeExtractor.Helpers
{
    public static class CollectionExtensions
    {
        public static void AddIfNotNull<T>(this ICollection<T> collection, T item) where T : class
        {
            if (collection != null)
            {
                if (item != null)
                {
                    if (!collection.Contains(item))
                    {
                        collection.Add(item);
                    }
                }
            }
        }
    }
}