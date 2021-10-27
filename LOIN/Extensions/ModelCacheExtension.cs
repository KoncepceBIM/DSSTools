using System;
using System.Collections.Generic;
using Xbim.Common;

namespace Xbim.Common
{
    public static class ModelCacheExtension
    {
        public static T GetCache<T>(this IModel model, string name) where T: class, new()
        {
            var tag = model.Tag;
            if (tag == null)
            {
                tag = new Dictionary<string, object>();
                model.Tag = tag;
            }

            if (!(tag is Dictionary<string, object> caches))
                throw new Exception("Unexpected tag type: " + tag.GetType().Name);

            if (caches.TryGetValue(name, out object cache))
            { 
                if (!(cache is T result))
                    throw new Exception("Unexpected return type: " + cache.GetType().Name);

                return result;
            }

            cache = new T();
            caches.Add(name, cache);
            return cache as T;
        }
    }
}
