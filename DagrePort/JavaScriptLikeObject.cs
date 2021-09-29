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

                return commonKeys;
                /*if (keysCached != null && !dirty)
                {
                    return keysCached;
                }
                dirty = false;
                keysCached = digitsKeysStr.Union(otherKeys).ToList();
                return keysCached;*/
                /*var l = orderedList.Select(z => z).ToList();

                var dgts = l.Where(z => z.All(char.IsDigit)).Where(z => (int.Parse(z) + "") == z).OrderBy(int.Parse).ToArray();
                l = dgts.Union(l.Except(dgts)).ToList();
                keysCached = l;
                return l;*/
            }
        }

        List<string> commonKeys = new List<string>();
        List<int> digitsKeys = new List<int>();
        //List<string> digitsKeysStr = new List<string>();
        List<string> otherKeys = new List<string>();

        void deleteKey(string key)
        {
            if (key.All(char.IsDigit) && int.Parse(key).ToString() == key)
            {
                var ind = binSearchInsertIndex(int.Parse(key));
                digitsKeys.RemoveAt(ind);
                //digitsKeysStr.RemoveAt(ind);
                commonKeys.RemoveAt(ind);
            }
            else
            {
                otherKeys.Remove(key);
                commonKeys.Remove(key);
            }
        }
        void insertKey(string key)
        {
            if (key.All(char.IsDigit) && int.Parse(key).ToString() == key)
            {
                var v = int.Parse(key);
                //binsearch
                int index = binSearchInsertIndex(v);
                digitsKeys.Insert(index, v);
               // digitsKeysStr.Insert(index, key);
                commonKeys.Insert(index, key);
            }
            else
            {
                otherKeys.Add(key);
                commonKeys.Add(key);
            }
        }

        private int binSearchInsertIndex(int key)
        {
            
            int low = 0;

            int high = digitsKeys.Count;
            while (true)
            {
                if (low >= high) break;
                var m = (high - low) / 2 + low;
                if (digitsKeys[m] < key)
                {
                    if (low == m)
                    {
                        return low + 1;
                    }
                    low = m;
                }
                else if (digitsKeys[m] == key)
                {
                    throw new DagreException("duplicate key");
                }
                else
                {
                    if (high == m) {
                        throw new DagreException("err");

                    }
                    high = m;
                }
                
            }
            return low;

        }

        /*bool dirty = true;
        List<string> keysCached;*/

        public ICollection<object> Values
        {
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
                //var ind1 = orderedList.IndexOf(fr);
                /* var ind1 = orderedListIndexes[key];
                 orderedList.RemoveAt(ind1);
                 orderedList.Insert(ind1, key);*/

                return;
            }
            //dirty = true;

            dic.Add(key, val);
            insertKey(key);
            //orderedList.Add(key);
            //orderedListIndexes.Add(key, orderedList.Count - 1);
            //if (dic.Keys.Count != orderedList.Count) throw new DagreException();
        }


        //List<string> orderedList = new List<string>();
        //Dictionary<string, int> orderedListIndexes = new Dictionary<string, int>();
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
            //dirty = true;
            //orderedList.Add(key);
            insertKey(key);
            //orderedListIndexes.Add(key, orderedList.Count - 1);
            dic.Add(key, value);
            //if (dic.Keys.Count != commonKeys.Count) throw new DagreException();
        }

        public bool Remove(string key)
        {
            if (_isFreezed) throw new DagreException("can't remove from frozen object");
            //var ind1 = orderedListIndexes[key];
            //var ind1 = orderedListIndexes[key];
            //dirty = true;
            //orderedList.RemoveAt(ind1);
            deleteKey(key);
            //orderedList.Remove(key);
            // orderedListIndexes.Remove(key);
            //  for (int i = 0; i < orderedList.Count; i++)
            {
                //orderedListIndexes[orderedList[i]] = i;
            }

            var ret = dic.Remove(key);
          //  if (dic.Keys.Count != commonKeys.Count) throw new DagreException();
            return ret;
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new System.NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (_isFreezed) return;
            //dirty = true;
            dic.Add(item.Key, item.Value);
            //orderedList.Add(item.Key);
            insertKey(item.Key);
            //orderedListIndexes.Add(item.Key, orderedList.Count - 1);
            //if (dic.Keys.Count != orderedList.Count) throw new DagreException();
        }

        public void Clear()
        {
            dic.Clear();
            otherKeys.Clear();
            digitsKeys.Clear();
            commonKeys.Clear();
            //digitsKeysStr.Clear();
            //orderedList.Clear();
            //dirty = true;
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
