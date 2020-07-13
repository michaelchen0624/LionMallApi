using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core
{
    public interface ICacheService
    {
        bool IsSet(string key);
        bool Set(string key, object value);
        bool Set(string key, object value, int cacheTime, bool isSliding = true);
        object Get(string key);
        T Get<T>(string key);
        bool Remove(string key);
    }
}
