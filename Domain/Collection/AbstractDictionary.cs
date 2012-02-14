using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Collection
{
    /// <summary>
    /// Decorator for IDictionary<K, T>.
    /// Used to be able to override methods 
    /// becuase there is no base class for this in BCL
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2010-04-29
    /// </remarks>
    public abstract class AbstractDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {

        private readonly IDictionary<TKey, TValue> _dictionary;

        protected AbstractDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item);
        }

        public virtual int Count
        {
            get { return _dictionary.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        public virtual bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public virtual void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public virtual bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public virtual TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        public virtual ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public virtual ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}