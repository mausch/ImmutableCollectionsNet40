using System;
using System.Collections.Generic;
using Validation;
namespace System.Collections.Immutable
{
	internal class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IEnumerator
	{
		private readonly IEnumerator<KeyValuePair<TKey, TValue>> inner;
		public DictionaryEntry Entry
		{
			get
			{
				KeyValuePair<TKey, TValue> current = this.inner.Current;
				object arg_30_0 = current.Key;
				KeyValuePair<TKey, TValue> current2 = this.inner.Current;
				return new DictionaryEntry(arg_30_0, current2.Value);
			}
		}
		public object Key
		{
			get
			{
				KeyValuePair<TKey, TValue> current = this.inner.Current;
				return current.Key;
			}
		}
		public object Value
		{
			get
			{
				KeyValuePair<TKey, TValue> current = this.inner.Current;
				return current.Value;
			}
		}
		public object Current
		{
			get
			{
				return this.Entry;
			}
		}
		internal DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> inner)
		{
			Requires.NotNull<IEnumerator<KeyValuePair<TKey, TValue>>>(inner, "inner");
			this.inner = inner;
		}
		public bool MoveNext()
		{
			return this.inner.MoveNext();
		}
		public void Reset()
		{
			this.inner.Reset();
		}
	}
}
