using System;
namespace System.Collections.Immutable
{
	internal class KeysCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TKey>
	{
		internal KeysCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary) : base(dictionary, dictionary.Keys)
		{
		}
		public override bool Contains(TKey item)
		{
			return base.Dictionary.ContainsKey(item);
		}
	}
}
