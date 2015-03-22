using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Runtime.Caching;
using System.Web;

namespace GenericRepositoryWithCatching.Models
{
    public class RepositoryCaching
    {
        public ObjectCache Cache
        {
            get { return MemoryCache.Default; }
        }

        public bool IsInMemory(string Key)
        {
            return Cache.Contains(Key);
        }

        public void Add(string Key, object Value, int Expiration)
        {
            Cache.Add(Key, Value, new CacheItemPolicy().AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(Expiration));
        }

        public List<T> FetchData<T>(string Key) where T : class
        {
            List<T> list = (List<T>)Cache[Key];
            return list;
        }

        public void Remove(string Key)
        {
            Cache.Remove(Key);
        }

    }
}
