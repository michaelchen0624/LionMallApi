//using Lion.Core.DoMain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prod.Core.Extension
{
    public class DictionaryExtension<TKey,TValue>
    {
        private Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        public bool Add(TKey tkey, TValue tv)
        {
            return dic.TryAdd(tkey, tv);
        }
        public void Clear()
        {
            dic.Clear();
        }
        public TValue this[TKey key]
        {
            get
            {
                if (dic.ContainsKey(key))
                    return dic[key];
                return default(TValue);
            }
        }
        public int Count
        {
            get
            {
                return dic.Count;
            }
        }
    }
}
