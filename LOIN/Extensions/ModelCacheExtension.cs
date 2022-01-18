using System;
using System.Collections.Concurrent;

namespace Xbim.Common
{
    public static class ModelCacheExtension
    {
        private static readonly object identity = new { };
        public static T GetCache<T>(this IModel model, string name, Func<T> factory) where T : class
        {
            var tag = model.Tag;
            if (tag == null)
            {
                lock (identity)
                {
                    if (tag == null)
                    {
                        tag = new ConcurrentDictionary<string, object>();
                        model.Tag = tag;
                    }
                }
            }

            if (!(tag is ConcurrentDictionary<string, object> caches))
                throw new Exception("Unexpected tag type: " + tag.GetType().Name);

            if (caches.TryGetValue(name, out object cache))
            {
                return !(cache is T result) ?
                    throw new Exception("Unexpected return type: " + cache.GetType().Name) :
                    result;
            }

            cache = caches.GetOrAdd(name, key => factory());
            return cache as T;
        }

        public static T GetCache<T>(this IModel model, string name) where T : class, new()
        {
            return GetCache(model, name, () => new T());
        }
    }
}
