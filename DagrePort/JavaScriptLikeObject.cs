using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public class JavaScriptLikeObject : IDictionary<string, object>
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        bool _isFreezed;


        //public ICollection<string> Keys => dic.Keys;
        public ICollection<string> Keys
        {
            get
            {
                var l = orderedList.Select(z => z.Key).ToList();
                var dgts = l.Where(z => z.All(char.IsDigit)).Where(z => (int.Parse(z) + "") == z).OrderBy(int.Parse).ToArray();
                l = dgts.Union(l.Except(dgts)).ToList();
                return l;
            }
        }

        public ICollection<object> Values {
            get
            {

                List<object> ret = new List<object>();
                foreach (var item in Keys)
                {
                    ret.Add(dic[item]);
                }
                return ret.ToArray();
            }
        
        }

        public int Count => dic.Count;

        public bool IsReadOnly => throw new System.NotImplementedException();

        public object this[string key]
        {
            get => dic[key];
            set
            {
                AddOrUpdate(key, value);
            }
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
                var fr = orderedList.First(z => z.Key == key);
                var ind1 = orderedList.IndexOf(fr);
                orderedList.Remove(fr);
                orderedList.Insert(ind1, new KeyValuePair<string, object>(key, val));

                return;
            }
            dic.Add(key, val);
            orderedList.Add(new KeyValuePair<string, object>(key, val));
            if (dic.Keys.Count != orderedList.Count) throw new DagreException();
        }

        List<KeyValuePair<string, object>> orderedList = new List<KeyValuePair<string, object>>();
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
            orderedList.Add(new KeyValuePair<string, object>(key, value));
            dic.Add(key, value);
            if (dic.Keys.Count != orderedList.Count) throw new DagreException();
        }

        public bool Remove(string key)
        {
            if (_isFreezed) throw new DagreException("can't remove from frozen object");
            orderedList.RemoveAll(z => z.Key == key);
            var ret = dic.Remove(key);
            if (dic.Keys.Count != orderedList.Count) throw new DagreException();
            return ret;
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new System.NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (_isFreezed) return;
            dic.Add(item.Key, item.Value);
            orderedList.Add(new KeyValuePair<string, object>(item.Key, item.Value));
            if (dic.Keys.Count != orderedList.Count) throw new DagreException();
        }

        public void Clear()
        {
            dic.Clear();
            orderedList.Clear();
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
            return dic.GetEnumerator();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dic.GetEnumerator();
        }
    }
}
