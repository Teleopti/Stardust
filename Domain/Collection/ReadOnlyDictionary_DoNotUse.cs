using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	//Use BCL's ReadOnlyDictionary instead!

	public class ReadOnlyDictionary_DoNotUse<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _wrappedDictionary;
		protected const string ExceptionString = "Cannot modify this dictionary. It is read only";

		public ReadOnlyDictionary_DoNotUse(IDictionary<TKey, TValue> wrappedDictionary)
		{
			InParameter.NotNull("wrappedDictionary", wrappedDictionary);
			_wrappedDictionary = wrappedDictionary;
		}

		public bool ContainsKey(TKey key)
		{
			return WrappedDictionary.ContainsKey(key);
		}

		public void Add(TKey key, TValue value)
		{
			throw new NotSupportedException(ExceptionString);
		}


		public bool Remove(TKey key)
		{
			throw new NotSupportedException(ExceptionString);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return WrappedDictionary.TryGetValue(key, out value);
		}

		public virtual TValue this[TKey key]
		{
			get
			{
				return WrappedDictionary[key];
			}
			set
			{
				throw new NotSupportedException(ExceptionString);
			}
		}

		public ICollection<TKey> Keys
		{
			get
			{
				return WrappedDictionary.Keys;
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				return WrappedDictionary.Values;
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException(ExceptionString);
		}

		public void Clear()
		{
			throw new NotSupportedException(ExceptionString);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return WrappedDictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			WrappedDictionary.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException(ExceptionString);
		}

		public int Count
		{
			get
			{
				return WrappedDictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		protected IDictionary<TKey, TValue> WrappedDictionary
		{
			get { return _wrappedDictionary; }
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return WrappedDictionary.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return WrappedDictionary.GetEnumerator();
		}
	}
}