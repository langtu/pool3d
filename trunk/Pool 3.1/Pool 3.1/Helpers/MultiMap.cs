using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XNA_PoolGame.Helpers
{
    /// <summary>
    /// Represents a map that might have multiple values to a key.
    /// </summary>
    public class MultiMap<TKey, TValue> : IEnumerable
    {
        #region Fields
//#if XBOX
        private Dictionary<TKey, List<TValue>> multimap;
        private int count;
        #endregion

        #region Constructor
        public MultiMap()
        {
            multimap = new Dictionary<TKey, List<TValue>>();
            count = 0;
        }
        #endregion

        #region Add/Remove

        /// <summary>
        /// Adds an element sorted to the MultiMap
        /// </summary>
        /// <param name="key">Key of the element</param>
        /// <param name="value">Value of the element</param>
        public void Add(TKey key, TValue value)
        {
            List<TValue> list;
            if (multimap.TryGetValue(key, out list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<TValue>();
                list.Add(value);
                multimap[key] = list;
            }
            
            ++count;
        }

        /// <summary>
        /// Adds an element sorted to the MultiMap. Use Key of IKey interface
        /// </summary>
        /// <param name="value"></param>
        public void Add(TValue value)
        {
            Add(((IKey<TKey>)value).Key, value);

        }

        /// <summary>
        /// Adds or set a element given the key.
        /// </summary>
        /// <param name="key">The Key of the value to set</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            set
            {
                List<TValue> list;
                if (multimap.TryGetValue(key, out list))
                {
                    TValue item;
                    bool found = false;
                    for (int i = 0; i < list.Count; ++i)
                    {
                        if (list[i].Equals(value))
                        {
                            item = list[i];
                            found = true;
                            break;
                        }
                    }
                    if (found) item = value;
                    else list.Add(value);
                }
                else
                {
                    list = new List<TValue>();
                    list.Add(value);
                    multimap[key] = list;
                }
                ++count;
            }
        }

        /// <summary>
        /// Removes a entire blocks of values given a key.
        /// </summary>
        /// <param name="key">The key to remove</param>
        public void Remove(TKey key)
        {
            multimap.Remove(key);
            --count;
        }

        /// <summary>
        /// Removes a value.
        /// </summary>
        /// <param name="value">The value to remove</param>
        public void Remove(TValue value)
        {
            List<TValue> list;
            if (multimap.TryGetValue(((IKey<TKey>)value).Key, out list))
            {
                list.Remove(value);
                if (list.Count == 0) multimap.Remove(((IKey<TKey>)value).Key);
            }
            --count;
        }

        #endregion

        #region KeyCollection
        /// <summary>
        /// Get the keys collection.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                return multimap.Keys;
            }
        }
        #endregion

        #region Count
        /// <summary>
        /// Get the total elements from the MultiMap.
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        #endregion

        #region Clear
        /// <summary>
        /// Removes all elements from the MultiMap.
        /// </summary>
        public void Clear()
        {
            multimap.Clear();
            count = 0;
        }
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns></returns>
       public IEnumerator GetEnumerator()
        {
            foreach (TKey key in multimap.Keys)
            {
                List<TValue> list = multimap[key];
                foreach (TValue element in list)
                    yield return element;
            }
        }

        #endregion

    }
}
