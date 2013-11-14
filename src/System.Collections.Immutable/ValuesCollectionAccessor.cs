using System;
namespace System.Collections.Immutable
{
	internal class ValuesCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TValue>
	{
		internal ValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary) : base(dictionary, dictionary.Values)
		{
		}
		public override bool Contains(TValue item)
		{
			ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary = base.Dictionary as ImmutableSortedDictionary<TKey, TValue>;
			if (immutableSortedDictionary != null)
			{
				return immutableSortedDictionary.ContainsValue(item);
			}
			ImmutableDictionary<TKey, TValue> immutableDictionary = base.Dictionary as ImmutableDictionary<TKey, TValue>;
			if (immutableDictionary != null)
			{
				return immutableDictionary.ContainsValue(item);
			}
			throw new NotSupportedException();
		}
	}
}
