using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SecondLive.Maker.Editor
{
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ISerializationCallbackReceiver, IDeserializationCallback
	{
		Dictionary<TKey, TValue> m_dict;

		[SerializeField] TKey[] m_keys;
		[SerializeField] TValue[] m_values;

		public SerializableDictionary()
		{
			m_dict = new Dictionary<TKey, TValue>();
		}

		public SerializableDictionary(IDictionary<TKey, TValue> dict)
		{
			m_dict = new Dictionary<TKey, TValue>(dict);
		}

		public void OnAfterDeserialize()
		{
			if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
			{
				m_dict.Clear();
				int n = m_keys.Length;
				for (int i = 0; i < n; ++i)
				{
					m_dict[m_keys[i]] = m_values[i];
				}
				m_keys = null;
				m_values = null;
			}
		}

		public void OnBeforeSerialize()
		{
			int n = m_dict.Count;
			m_keys = new TKey[n];
			m_values = new TValue[n];

			int i = 0;
			foreach (var kvp in m_dict)
			{
				m_keys[i] = kvp.Key;
				m_values[i] = kvp.Value;
				++i;
			}
		}

		#region IDictionary<TKey, TValue>

		public ICollection<TKey> Keys { get { return ((IDictionary<TKey, TValue>)m_dict).Keys; } }
		public ICollection<TValue> Values { get { return ((IDictionary<TKey, TValue>)m_dict).Values; } }
		public int Count { get { return ((IDictionary<TKey, TValue>)m_dict).Count; } }
		public bool IsReadOnly { get { return ((IDictionary<TKey, TValue>)m_dict).IsReadOnly; } }

		public TValue this[TKey key]
		{
			get { return ((IDictionary<TKey, TValue>)m_dict)[key]; }
			set { ((IDictionary<TKey, TValue>)m_dict)[key] = value; }
		}

		public void Add(TKey key, TValue value)
		{
			((IDictionary<TKey, TValue>)m_dict).Add(key, value);
		}

		public bool ContainsKey(TKey key)
		{
			return ((IDictionary<TKey, TValue>)m_dict).ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			return ((IDictionary<TKey, TValue>)m_dict).Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return ((IDictionary<TKey, TValue>)m_dict).TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			((IDictionary<TKey, TValue>)m_dict).Add(item);
		}

		public void Clear()
		{
			((IDictionary<TKey, TValue>)m_dict).Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((IDictionary<TKey, TValue>)m_dict).Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((IDictionary<TKey, TValue>)m_dict).CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return ((IDictionary<TKey, TValue>)m_dict).Remove(item);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
		}

		#endregion

		#region IDictionary

		public bool IsFixedSize { get { return ((IDictionary)m_dict).IsFixedSize; } }
		ICollection IDictionary.Keys { get { return ((IDictionary)m_dict).Keys; } }
		ICollection IDictionary.Values { get { return ((IDictionary)m_dict).Values; } }
		public bool IsSynchronized { get { return ((IDictionary)m_dict).IsSynchronized; } }
		public object SyncRoot { get { return ((IDictionary)m_dict).SyncRoot; } }

		public object this[object key]
		{
			get { return ((IDictionary)m_dict)[key]; }
			set { ((IDictionary)m_dict)[key] = value; }
		}

		public void Add(object key, object value)
		{
			((IDictionary)m_dict).Add(key, value);
		}

		public bool Contains(object key)
		{
			return ((IDictionary)m_dict).Contains(key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return ((IDictionary)m_dict).GetEnumerator();
		}

		public void Remove(object key)
		{
			((IDictionary)m_dict).Remove(key);
		}

		public void CopyTo(Array array, int index)
		{
			((IDictionary)m_dict).CopyTo(array, index);
		}

		#endregion

		#region IDeserializationCallback

		public void OnDeserialization(object sender)
		{
			((IDeserializationCallback)m_dict).OnDeserialization(sender);
		}

		#endregion
	}
}

