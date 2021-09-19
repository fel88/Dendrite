using System;
using System.Collections;
using System.Collections.Generic;

namespace Dagre
{
    public class JavaScriptLikeObject : IDictionary<string, object>
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        bool _isFreezed;

        public ICollection<string> Keys => dic.Keys;

        public ICollection<object> Values => dic.Values;

        public int Count => dic.Count;

        public bool IsReadOnly => throw new System.NotImplementedException();

        public object this[string key]
        {
            get => dic[key];
            set => dic[key] = value;
        }
        public void Freeze()
        {
            _isFreezed = true;
        }
        public void AddOrUpdate(string key, object val)
        {
            if (_isFreezed) throw new DagreException("can't add to frozen object");
            if (dic.ContainsKey(key))
            {
                dic[key] = val;
                return;
            }
            dic.Add(key, val);
        }

        public override string ToString()
        {
            return $"dic ({dic.Keys.Count})";
        }

        public bool ContainsKey(string key)
        {
            return dic.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            if (_isFreezed) throw new DagreException("can't add to frozen object");

            dic.Add(key, value);
        }

        public bool Remove(string key)
        {
            if (_isFreezed) throw new DagreException("can't remove from frozen object");
            return dic.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new System.NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            dic.Add(item.Key,item.Value);
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }



        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {            
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.GetEnumerator();
        }
    }
}
