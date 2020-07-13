using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Prod.Core
{
    public class MemoryCacheService : ICacheService
    {
        private IMemoryCache _cache;
        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            object obj = Get(key);
            return obj == null ? default(T) : (T)obj;
        }

        public object Get(string key)
        {
            object val = null;
            if (key != null && _cache.TryGetValue(key, out val))
            {
                return val;
            }
            else
                return default(object);
        }

        public bool IsSet(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            object value;
            return _cache.TryGetValue(key, out value);
        }

        public bool Remove(string key)
        {
            if (key == null)
                return true;
            else
            {
                _cache.Remove(key);
            }
            return !IsSet(key);
        }

        public bool Set(string key, object value)
        {
            if(key!=null)
            {
                _cache.Set(key, value);
            }           
            return IsSet(key);
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheTime">缓存过期时间(秒)</param>
        /// <param name="isSliding">是否滑动</param>
        /// <returns></returns>
        public bool Set(string key, object value, int cacheTime, bool isSliding = true)
        {
            if(key!=null)
            {
                if(isSliding)
                {
                    _cache.Set(key, value, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheTime)));
                }
                else
                {
                    _cache.Set(key, value, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheTime)));
                }              
            }
            return IsSet(key);
        }
    }
}
