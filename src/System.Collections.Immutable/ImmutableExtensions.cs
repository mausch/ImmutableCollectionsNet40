using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Validation;
namespace System.Collections.Immutable
{
	internal static class ImmutableExtensions
	{
		private class ListOfTWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
		{
			private readonly IList<T> collection;
			public int Count
			{
				get
				{
					return this.collection.Count;
				}
			}
			public T this[int index]
			{
				get
				{
					return this.collection[index];
				}
			}
			internal ListOfTWrapper(IList<T> collection)
			{
				Requires.NotNull<IList<T>>(collection, "collection");
				this.collection = collection;
			}
			public IEnumerator<T> GetEnumerator()
			{
				return this.collection.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}
		private class FallbackWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
		{
			private readonly IEnumerable<T> sequence;
			private IList<T> collection;
			public int Count
			{
				get
				{
					if (this.collection == null)
					{
						int result;
						if (this.sequence.TryGetCount(out result))
						{
							return result;
						}
						this.collection = this.sequence.ToArray<T>();
					}
					return this.collection.Count;
				}
			}
			public T this[int index]
			{
				get
				{
					if (this.collection == null)
					{
						this.collection = this.sequence.ToArray<T>();
					}
					return this.collection[index];
				}
			}
			internal FallbackWrapper(IEnumerable<T> sequence)
			{
				Requires.NotNull<IEnumerable<T>>(sequence, "sequence");
				this.sequence = sequence;
			}
			public IEnumerator<T> GetEnumerator()
			{
				return this.sequence.GetEnumerator();
			}
			[ExcludeFromCodeCoverage]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}
		internal static bool TryGetCount<T>(this IEnumerable<T> sequence, out int count)
		{
			return sequence.TryGetCount(out count);
		}
		internal static bool TryGetCount<T>(this IEnumerable sequence, out int count)
		{
			ICollection collection = sequence as ICollection;
			if (collection != null)
			{
				count = collection.Count;
				return true;
			}
			ICollection<T> collection2 = sequence as ICollection<T>;
			if (collection2 != null)
			{
				count = collection2.Count;
				return true;
			}
			IReadOnlyCollection<T> readOnlyCollection = sequence as IReadOnlyCollection<T>;
			if (readOnlyCollection != null)
			{
				count = readOnlyCollection.Count;
				return true;
			}
			count = 0;
			return false;
		}
		internal static int GetCount<T>(ref IEnumerable<T> sequence)
		{
			int count;
			if (!sequence.TryGetCount(out count))
			{
				List<T> list = sequence.ToList<T>();
				count = list.Count;
				sequence = list;
			}
			return count;
		}
		internal static T[] ToArray<T>(this IEnumerable<T> sequence, int count)
		{
			Requires.NotNull<IEnumerable<T>>(sequence, "sequence");
			Requires.Range(count >= 0, "count", null);
			T[] array = new T[count];
			int num = 0;
			foreach (T current in sequence)
			{
				Requires.Argument(num < count);
				array[num++] = current;
			}
			Requires.Argument(num == count);
			return array;
		}
		internal static IOrderedCollection<T> AsOrderedCollection<T>(this IEnumerable<T> sequence)
		{
			Requires.NotNull<IEnumerable<T>>(sequence, "sequence");
			IOrderedCollection<T> orderedCollection = sequence as IOrderedCollection<T>;
			if (orderedCollection != null)
			{
				return orderedCollection;
			}
			IList<T> list = sequence as IList<T>;
			if (list != null)
			{
				return new ImmutableExtensions.ListOfTWrapper<T>(list);
			}
			return new ImmutableExtensions.FallbackWrapper<T>(sequence);
		}
	}
}
