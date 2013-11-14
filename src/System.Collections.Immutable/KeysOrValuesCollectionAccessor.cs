using System;
using System.Collections.Generic;
using System.Diagnostics;
using Validation;
namespace System.Collections.Immutable
{
	internal abstract class KeysOrValuesCollectionAccessor<TKey, TValue, T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
	{
		private readonly IImmutableDictionary<TKey, TValue> dictionary;
		private readonly IEnumerable<T> keysOrValues;
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}
		public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}
		protected IImmutableDictionary<TKey, TValue> Dictionary
		{
			get
			{
				return this.dictionary;
			}
		}
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			get
			{
				return true;
			}
		}
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot
		{
			get
			{
				return this;
			}
		}
		protected KeysOrValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary, IEnumerable<T> keysOrValues)
		{
			Requires.NotNull<IImmutableDictionary<TKey, TValue>>(dictionary, "dictionary");
			Requires.NotNull<IEnumerable<T>>(keysOrValues, "keysOrValues");
			this.dictionary = dictionary;
			this.keysOrValues = keysOrValues;
		}
		public void Add(T item)
		{
			throw new NotSupportedException();
		}
		public void Clear()
		{
			throw new NotSupportedException();
		}
		public abstract bool Contains(T item);
		public void CopyTo(T[] array, int arrayIndex)
		{
			Requires.NotNull<T[]>(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex", null);
			Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
			foreach (T current in this)
			{
				array[arrayIndex++] = current;
			}
		}
		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}
		public IEnumerator<T> GetEnumerator()
		{
			return this.keysOrValues.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			Requires.NotNull<Array>(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex", null);
			Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
			foreach (T current in this)
			{
				array.SetValue(current, new int[]
				{
					arrayIndex++
				});
			}
		}
	}
}
