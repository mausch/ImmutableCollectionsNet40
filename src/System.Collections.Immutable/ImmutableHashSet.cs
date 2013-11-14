using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Validation;
namespace System.Collections.Immutable
{
	public static class ImmutableHashSet
	{
		public static ImmutableHashSet<T> Create<T>()
		{
			return ImmutableHashSet<T>.Empty;
		}
		public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> equalityComparer)
		{
			return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer);
		}
		public static ImmutableHashSet<T> Create<T>(T item)
		{
			return ImmutableHashSet<T>.Empty.Add(item);
		}
		public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> equalityComparer, T item)
		{
			return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Add(item);
		}
		public static ImmutableHashSet<T> CreateRange<T>(IEnumerable<T> items)
		{
			return ImmutableHashSet<T>.Empty.Union(items);
		}
		public static ImmutableHashSet<T> CreateRange<T>(IEqualityComparer<T> equalityComparer, IEnumerable<T> items)
		{
			return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
		}
		public static ImmutableHashSet<T> Create<T>(params T[] items)
		{
			return ImmutableHashSet<T>.Empty.Union(items);
		}
		public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T> equalityComparer, params T[] items)
		{
			return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
		}
		public static ImmutableHashSet<T>.Builder CreateBuilder<T>()
		{
			return ImmutableHashSet.Create<T>().ToBuilder();
		}
		public static ImmutableHashSet<T>.Builder CreateBuilder<T>(IEqualityComparer<T> equalityComparer)
		{
			return ImmutableHashSet.Create<T>(equalityComparer).ToBuilder();
		}
		public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> equalityComparer)
		{
			ImmutableHashSet<TSource> immutableHashSet = source as ImmutableHashSet<TSource>;
			if (immutableHashSet != null)
			{
				return immutableHashSet.WithComparer(equalityComparer);
			}
			return ImmutableHashSet<TSource>.Empty.WithComparer(equalityComparer).Union(source);
		}
		public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(this IEnumerable<TSource> source)
		{
			return source.ToImmutableHashSet(null);
		}
	}
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableHashSet<>.DebuggerProxy))]
	public sealed class ImmutableHashSet<T> : IImmutableSet<T>, IHashKeyCollection<T>, IReadOnlyCollection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
	{
		[DebuggerDisplay("Count = {Count}")]
		public sealed class Builder : IReadOnlyCollection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
		{
			private ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root = ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
			private IEqualityComparer<T> equalityComparer;
			private int count;
			private ImmutableHashSet<T> immutable;
			private int version;
			public int Count
			{
				get
				{
					return this.count;
				}
			}
			bool ICollection<T>.IsReadOnly
			{
				get
				{
					return false;
				}
			}
			public IEqualityComparer<T> KeyComparer
			{
				get
				{
					return this.equalityComparer;
				}
				set
				{
					Requires.NotNull<IEqualityComparer<T>>(value, "value");
					if (value != this.equalityComparer)
					{
						ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Union(this, new ImmutableHashSet<T>.MutationInput(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode, value, 0));
						this.immutable = null;
						this.equalityComparer = value;
						this.Root = mutationResult.Root;
						this.count = mutationResult.Count;
					}
				}
			}
			internal int Version
			{
				get
				{
					return this.version;
				}
			}
			private ImmutableHashSet<T>.MutationInput Origin
			{
				get
				{
					return new ImmutableHashSet<T>.MutationInput(this.Root, this.equalityComparer, this.count);
				}
			}
			private ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node Root
			{
				get
				{
					return this.root;
				}
				set
				{
					this.version++;
					if (this.root != value)
					{
						this.root = value;
						this.immutable = null;
					}
				}
			}
			internal Builder(ImmutableHashSet<T> set)
			{
				Requires.NotNull<ImmutableHashSet<T>>(set, "set");
				this.root = set.root;
				this.count = set.count;
				this.equalityComparer = set.equalityComparer;
				this.immutable = set;
			}
			public ImmutableHashSet<T>.Enumerator GetEnumerator()
			{
				return new ImmutableHashSet<T>.Enumerator(this.root, this);
			}
			public ImmutableHashSet<T> ToImmutable()
			{
				if (this.immutable == null)
				{
					this.immutable = ImmutableHashSet<T>.Wrap(this.root, this.equalityComparer, this.count);
				}
				return this.immutable;
			}
			public bool Add(T item)
			{
				ImmutableHashSet<T>.MutationResult result = ImmutableHashSet<T>.Add(item, this.Origin);
				this.Apply(result);
				return result.Count != 0;
			}
			public bool Remove(T item)
			{
				ImmutableHashSet<T>.MutationResult result = ImmutableHashSet<T>.Remove(item, this.Origin);
				this.Apply(result);
				return result.Count != 0;
			}
			public bool Contains(T item)
			{
				return ImmutableHashSet<T>.Contains(item, this.Origin);
			}
			public void Clear()
			{
				this.count = 0;
				this.Root = ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
			}
			public void ExceptWith(IEnumerable<T> other)
			{
				ImmutableHashSet<T>.MutationResult result = ImmutableHashSet<T>.Except(other, this.equalityComparer, this.root);
				this.Apply(result);
			}
			public void IntersectWith(IEnumerable<T> other)
			{
				ImmutableHashSet<T>.MutationResult result = ImmutableHashSet<T>.Intersect(other, this.Origin);
				this.Apply(result);
			}
			public bool IsProperSubsetOf(IEnumerable<T> other)
			{
				return ImmutableHashSet<T>.IsProperSubsetOf(other, this.Origin);
			}
			public bool IsProperSupersetOf(IEnumerable<T> other)
			{
				return ImmutableHashSet<T>.IsProperSupersetOf(other, this.Origin);
			}
			public bool IsSubsetOf(IEnumerable<T> other)
			{
				return ImmutableHashSet<T>.IsSubsetOf(other, this.Origin);
			}
			public bool IsSupersetOf(IEnumerable<T> other)
			{
				return ImmutableHashSet<T>.IsSupersetOf(other, this.Origin);
			}
			public bool Overlaps(IEnumerable<T> other)
			{
				return ImmutableHashSet<T>.Overlaps(other, this.Origin);
			}
			public bool SetEquals(IEnumerable<T> other)
			{
				return ImmutableHashSet<T>.SetEquals(other, this.Origin);
			}
			public void SymmetricExceptWith(IEnumerable<T> other)
			{
				ImmutableHashSet<T>.MutationResult result = ImmutableHashSet<T>.SymmetricExcept(other, this.Origin);
				this.Apply(result);
			}
			public void UnionWith(IEnumerable<T> other)
			{
				ImmutableHashSet<T>.MutationResult result = ImmutableHashSet<T>.Union(other, this.Origin);
				this.Apply(result);
			}
			void ICollection<T>.Add(T item)
			{
				this.Add(item);
			}
			void ICollection<T>.CopyTo(T[] array, int arrayIndex)
			{
				Requires.NotNull<T[]>(array, "array");
				Requires.Range(arrayIndex >= 0, "arrayIndex", null);
				Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
				foreach (T current in this)
				{
					array[arrayIndex++] = current;
				}
			}
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			private void Apply(ImmutableHashSet<T>.MutationResult result)
			{
				this.Root = result.Root;
				if (result.CountType == ImmutableHashSet<T>.CountType.Adjustment)
				{
					this.count += result.Count;
					return;
				}
				this.count = result.Count;
			}
		}
		[ExcludeFromCodeCoverage]
		private class DebuggerProxy
		{
			private readonly ImmutableHashSet<T> set;
			private T[] contents;
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public T[] Contents
			{
				get
				{
					if (this.contents == null)
					{
						this.contents = this.set.ToArray(this.set.Count);
					}
					return this.contents;
				}
			}
			public DebuggerProxy(ImmutableHashSet<T> set)
			{
				Requires.NotNull<ImmutableHashSet<T>>(set, "set");
				this.set = set;
			}
		}
		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly ImmutableHashSet<T>.Builder builder;
			private ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Enumerator mapEnumerator;
			private ImmutableHashSet<T>.HashBucket.Enumerator bucketEnumerator;
			private int enumeratingBuilderVersion;
			public T Current
			{
				get
				{
					this.mapEnumerator.ThrowIfDisposed();
					return this.bucketEnumerator.Current;
				}
			}
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			internal Enumerator(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, ImmutableHashSet<T>.Builder builder = null)
			{
				this.builder = builder;
				this.mapEnumerator = new ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Enumerator(root, null);
				this.bucketEnumerator = default(ImmutableHashSet<T>.HashBucket.Enumerator);
				this.enumeratingBuilderVersion = ((builder != null) ? builder.Version : -1);
			}
			public bool MoveNext()
			{
				this.ThrowIfChanged();
				if (this.bucketEnumerator.MoveNext())
				{
					return true;
				}
				if (this.mapEnumerator.MoveNext())
				{
					KeyValuePair<int, ImmutableHashSet<T>.HashBucket> current = this.mapEnumerator.Current;
					this.bucketEnumerator = new ImmutableHashSet<T>.HashBucket.Enumerator(current.Value);
					return this.bucketEnumerator.MoveNext();
				}
				return false;
			}
			public void Reset()
			{
				this.enumeratingBuilderVersion = ((this.builder != null) ? this.builder.Version : -1);
				this.mapEnumerator.Reset();
				this.bucketEnumerator.Dispose();
				this.bucketEnumerator = default(ImmutableHashSet<T>.HashBucket.Enumerator);
			}
			public void Dispose()
			{
				this.mapEnumerator.Dispose();
				this.bucketEnumerator.Dispose();
			}
			private void ThrowIfChanged()
			{
				if (this.builder != null && this.builder.Version != this.enumeratingBuilderVersion)
				{
					throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
				}
			}
		}
		internal enum OperationResult
		{
			SizeChanged,
			NoChangeRequired
		}
		internal struct HashBucket
		{
			internal struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
			{
				private enum Position
				{
					BeforeFirst,
					First,
					Additional,
					End
				}
				private readonly ImmutableHashSet<T>.HashBucket bucket;
				private bool disposed;
				private ImmutableHashSet<T>.HashBucket.Enumerator.Position currentPosition;
				private ImmutableList<T>.Enumerator additionalEnumerator;
				object IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}
				public T Current
				{
					get
					{
						this.ThrowIfDisposed();
						switch (this.currentPosition)
						{
						case ImmutableHashSet<T>.HashBucket.Enumerator.Position.First:
							return this.bucket.firstValue;
						case ImmutableHashSet<T>.HashBucket.Enumerator.Position.Additional:
							return this.additionalEnumerator.Current;
						default:
							throw new InvalidOperationException();
						}
					}
				}
				internal Enumerator(ImmutableHashSet<T>.HashBucket bucket)
				{
					this.disposed = false;
					this.bucket = bucket;
					this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.BeforeFirst;
					this.additionalEnumerator = default(ImmutableList<T>.Enumerator);
				}
				public bool MoveNext()
				{
					this.ThrowIfDisposed();
					if (this.bucket.IsEmpty)
					{
						this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.End;
						return false;
					}
					switch (this.currentPosition)
					{
					case ImmutableHashSet<T>.HashBucket.Enumerator.Position.BeforeFirst:
						this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.First;
						return true;
					case ImmutableHashSet<T>.HashBucket.Enumerator.Position.First:
						if (this.bucket.additionalElements.IsEmpty)
						{
							this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.End;
							return false;
						}
						this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.Additional;
						this.additionalEnumerator = new ImmutableList<T>.Enumerator(this.bucket.additionalElements, null, -1, -1, false);
						return this.additionalEnumerator.MoveNext();
					case ImmutableHashSet<T>.HashBucket.Enumerator.Position.Additional:
						return this.additionalEnumerator.MoveNext();
					case ImmutableHashSet<T>.HashBucket.Enumerator.Position.End:
						return false;
					default:
						throw new InvalidOperationException();
					}
				}
				public void Reset()
				{
					this.ThrowIfDisposed();
					this.additionalEnumerator.Dispose();
					this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.BeforeFirst;
				}
				public void Dispose()
				{
					this.disposed = true;
					this.additionalEnumerator.Dispose();
				}
				private void ThrowIfDisposed()
				{
					if (this.disposed)
					{
						throw new ObjectDisposedException(base.GetType().FullName);
					}
				}
			}
			private readonly T firstValue;
			private readonly ImmutableList<T>.Node additionalElements;
			internal bool IsEmpty
			{
				get
				{
					return this.additionalElements == null;
				}
			}
			private HashBucket(T firstElement, ImmutableList<T>.Node additionalElements = null)
			{
				this.firstValue = firstElement;
				this.additionalElements = (additionalElements ?? ImmutableList<T>.Node.EmptyNode);
			}
			public ImmutableHashSet<T>.HashBucket.Enumerator GetEnumerator()
			{
				return new ImmutableHashSet<T>.HashBucket.Enumerator(this);
			}
			internal ImmutableHashSet<T>.HashBucket Add(T value, IEqualityComparer<T> valueComparer, out ImmutableHashSet<T>.OperationResult result)
			{
				if (this.IsEmpty)
				{
					result = ImmutableHashSet<T>.OperationResult.SizeChanged;
					return new ImmutableHashSet<T>.HashBucket(value, null);
				}
				if (valueComparer.Equals(value, this.firstValue) || this.additionalElements.IndexOf(value, valueComparer) >= 0)
				{
					result = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
					return this;
				}
				result = ImmutableHashSet<T>.OperationResult.SizeChanged;
				return new ImmutableHashSet<T>.HashBucket(this.firstValue, this.additionalElements.Add(value));
			}
			internal bool Contains(T value, IEqualityComparer<T> valueComparer)
			{
				return !this.IsEmpty && (valueComparer.Equals(value, this.firstValue) || this.additionalElements.IndexOf(value, valueComparer) >= 0);
			}
			internal bool TryExchange(T value, IEqualityComparer<T> valueComparer, out T existingValue)
			{
				if (!this.IsEmpty)
				{
					if (valueComparer.Equals(value, this.firstValue))
					{
						existingValue = this.firstValue;
						return true;
					}
					int num = this.additionalElements.IndexOf(value, valueComparer);
					if (num >= 0)
					{
						existingValue = this.additionalElements[num];
						return true;
					}
				}
				existingValue = value;
				return false;
			}
			internal ImmutableHashSet<T>.HashBucket Remove(T value, IEqualityComparer<T> equalityComparer, out ImmutableHashSet<T>.OperationResult result)
			{
				if (this.IsEmpty)
				{
					result = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
					return this;
				}
				if (equalityComparer.Equals(this.firstValue, value))
				{
					if (this.additionalElements.IsEmpty)
					{
						result = ImmutableHashSet<T>.OperationResult.SizeChanged;
						return default(ImmutableHashSet<T>.HashBucket);
					}
					int count = ((IBinaryTree<T>)this.additionalElements).Left.Count;
					result = ImmutableHashSet<T>.OperationResult.SizeChanged;
					return new ImmutableHashSet<T>.HashBucket(this.additionalElements.Key, this.additionalElements.RemoveAt(count));
				}
				else
				{
					int num = this.additionalElements.IndexOf(value, equalityComparer);
					if (num < 0)
					{
						result = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
						return this;
					}
					result = ImmutableHashSet<T>.OperationResult.SizeChanged;
					return new ImmutableHashSet<T>.HashBucket(this.firstValue, this.additionalElements.RemoveAt(num));
				}
			}
			internal void Freeze()
			{
				if (this.additionalElements != null)
				{
					this.additionalElements.Freeze();
				}
			}
		}
		private struct MutationInput
		{
			private readonly ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root;
			private readonly IEqualityComparer<T> equalityComparer;
			private readonly int count;
			internal ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node Root
			{
				get
				{
					return this.root;
				}
			}
			internal IEqualityComparer<T> EqualityComparer
			{
				get
				{
					return this.equalityComparer;
				}
			}
			internal int Count
			{
				get
				{
					return this.count;
				}
			}
			internal MutationInput(ImmutableHashSet<T> set)
			{
				Requires.NotNull<ImmutableHashSet<T>>(set, "set");
				this.root = set.root;
				this.equalityComparer = set.equalityComparer;
				this.count = set.count;
			}
			internal MutationInput(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, IEqualityComparer<T> equalityComparer, int count)
			{
				Requires.NotNull<ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
				Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
				Requires.Range(count >= 0, "count", null);
				this.root = root;
				this.equalityComparer = equalityComparer;
				this.count = count;
			}
		}
		private enum CountType
		{
			Adjustment,
			FinalValue
		}
		private struct MutationResult
		{
			private readonly ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root;
			private readonly int count;
			private readonly ImmutableHashSet<T>.CountType countType;
			internal ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node Root
			{
				get
				{
					return this.root;
				}
			}
			internal int Count
			{
				get
				{
					return this.count;
				}
			}
			internal ImmutableHashSet<T>.CountType CountType
			{
				get
				{
					return this.countType;
				}
			}
			internal MutationResult(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, int count, ImmutableHashSet<T>.CountType countType = ImmutableHashSet<T>.CountType.Adjustment)
			{
				Requires.NotNull<ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
				this.root = root;
				this.count = count;
				this.countType = countType;
			}
			internal ImmutableHashSet<T> Finalize(ImmutableHashSet<T> priorSet)
			{
				Requires.NotNull<ImmutableHashSet<T>>(priorSet, "priorSet");
				int num = this.Count;
				if (this.CountType == ImmutableHashSet<T>.CountType.Adjustment)
				{
					num += priorSet.count;
				}
				return priorSet.Wrap(this.Root, num);
			}
		}
		private struct NodeEnumerable : IEnumerable<T>, IEnumerable
		{
			private readonly ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root;
			internal NodeEnumerable(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root)
			{
				Requires.NotNull<ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
				this.root = root;
			}
			public ImmutableHashSet<T>.Enumerator GetEnumerator()
			{
				return new ImmutableHashSet<T>.Enumerator(this.root, null);
			}
			[ExcludeFromCodeCoverage]
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			[ExcludeFromCodeCoverage]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}
		public static readonly ImmutableHashSet<T> Empty = new ImmutableHashSet<T>(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode, EqualityComparer<T>.Default, 0);
		private static readonly Action<KeyValuePair<int, ImmutableHashSet<T>.HashBucket>> FreezeBucketAction = delegate(KeyValuePair<int, ImmutableHashSet<T>.HashBucket> kv)
		{
			kv.Value.Freeze();
		};
		private readonly IEqualityComparer<T> equalityComparer;
		private readonly int count;
		private readonly ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root;
		public int Count
		{
			get
			{
				return this.count;
			}
		}
		public bool IsEmpty
		{
			get
			{
				return this.Count == 0;
			}
		}
		public IEqualityComparer<T> KeyComparer
		{
			get
			{
				return this.equalityComparer;
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
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			get
			{
				return true;
			}
		}
		private ImmutableHashSet<T>.MutationInput Origin
		{
			get
			{
				return new ImmutableHashSet<T>.MutationInput(this);
			}
		}
		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return true;
			}
		}
		internal ImmutableHashSet(IEqualityComparer<T> equalityComparer) : this(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode, equalityComparer, 0)
		{
		}
		private ImmutableHashSet(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, IEqualityComparer<T> equalityComparer, int count)
		{
			Requires.NotNull<ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
			Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
			root.Freeze(ImmutableHashSet<T>.FreezeBucketAction);
			this.root = root;
			this.count = count;
			this.equalityComparer = equalityComparer;
		}
		public ImmutableHashSet<T> Clear()
		{
			if (!this.IsEmpty)
			{
				return ImmutableHashSet<T>.Empty.WithComparer(this.equalityComparer);
			}
			return this;
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Clear()
		{
			return this.Clear();
		}
		public ImmutableHashSet<T>.Builder ToBuilder()
		{
			return new ImmutableHashSet<T>.Builder(this);
		}
		public ImmutableHashSet<T> Add(T item)
		{
			Requires.NotNullAllowStructs<T>(item, "item");
			return ImmutableHashSet<T>.Add(item, this.Origin).Finalize(this);
		}
		public ImmutableHashSet<T> Remove(T item)
		{
			Requires.NotNullAllowStructs<T>(item, "item");
			return ImmutableHashSet<T>.Remove(item, this.Origin).Finalize(this);
		}
		public bool TryGetValue(T equalValue, out T actualValue)
		{
			Requires.NotNullAllowStructs<T>(equalValue, "value");
			int hashCode = this.equalityComparer.GetHashCode(equalValue);
			ImmutableHashSet<T>.HashBucket hashBucket;
			if (this.root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
			{
				return hashBucket.TryExchange(equalValue, this.equalityComparer, out actualValue);
			}
			actualValue = equalValue;
			return false;
		}
		public ImmutableHashSet<T> Union(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return this.Union(other, false);
		}
		public ImmutableHashSet<T> Intersect(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.Intersect(other, this.Origin).Finalize(this);
		}
		public ImmutableHashSet<T> Except(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.Except(other, this.equalityComparer, this.root).Finalize(this);
		}
		public ImmutableHashSet<T> SymmetricExcept(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.SymmetricExcept(other, this.Origin).Finalize(this);
		}
		public bool SetEquals(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.SetEquals(other, this.Origin);
		}
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.IsProperSubsetOf(other, this.Origin);
		}
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.IsProperSupersetOf(other, this.Origin);
		}
		public bool IsSubsetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.IsSubsetOf(other, this.Origin);
		}
		public bool IsSupersetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.IsSupersetOf(other, this.Origin);
		}
		public bool Overlaps(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			return ImmutableHashSet<T>.Overlaps(other, this.Origin);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Add(T item)
		{
			return this.Add(item);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Remove(T item)
		{
			return this.Remove(item);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
		{
			return this.Union(other);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
		{
			return this.Intersect(other);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
		{
			return this.Except(other);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
		{
			return this.SymmetricExcept(other);
		}
		public bool Contains(T item)
		{
			Requires.NotNullAllowStructs<T>(item, "item");
			return ImmutableHashSet<T>.Contains(item, this.Origin);
		}
		public ImmutableHashSet<T> WithComparer(IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null)
			{
				equalityComparer = EqualityComparer<T>.Default;
			}
			if (equalityComparer == this.equalityComparer)
			{
				return this;
			}
			ImmutableHashSet<T> immutableHashSet = new ImmutableHashSet<T>(equalityComparer);
			return immutableHashSet.Union(this, true);
		}
		bool ISet<T>.Add(T item)
		{
			throw new NotSupportedException();
		}
		void ISet<T>.ExceptWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}
		void ISet<T>.IntersectWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}
		void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}
		void ISet<T>.UnionWith(IEnumerable<T> other)
		{
			throw new NotSupportedException();
		}
		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			Requires.NotNull<T[]>(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex", null);
			Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
			foreach (T current in this)
			{
				array[arrayIndex++] = current;
			}
		}
		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException();
		}
		void ICollection<T>.Clear()
		{
			throw new NotSupportedException();
		}
		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
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
		public ImmutableHashSet<T>.Enumerator GetEnumerator()
		{
			return new ImmutableHashSet<T>.Enumerator(this.root, null);
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		private static bool IsSupersetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			foreach (T current in other)
			{
				if (!ImmutableHashSet<T>.Contains(current, origin))
				{
					return false;
				}
			}
			return true;
		}
		private static ImmutableHashSet<T>.MutationResult Add(T item, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNullAllowStructs<T>(item, "item");
			int hashCode = origin.EqualityComparer.GetHashCode(item);
			ImmutableHashSet<T>.OperationResult operationResult;
			ImmutableHashSet<T>.HashBucket newBucket = origin.Root.GetValueOrDefault(hashCode, Comparer<int>.Default).Add(item, origin.EqualityComparer, out operationResult);
			if (operationResult == ImmutableHashSet<T>.OperationResult.NoChangeRequired)
			{
				return new ImmutableHashSet<T>.MutationResult(origin.Root, 0, ImmutableHashSet<T>.CountType.Adjustment);
			}
			ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node node = ImmutableHashSet<T>.UpdateRoot(origin.Root, hashCode, newBucket);
			return new ImmutableHashSet<T>.MutationResult(node, 1, ImmutableHashSet<T>.CountType.Adjustment);
		}
		private static ImmutableHashSet<T>.MutationResult Remove(T item, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNullAllowStructs<T>(item, "item");
			ImmutableHashSet<T>.OperationResult operationResult = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
			int hashCode = origin.EqualityComparer.GetHashCode(item);
			ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node node = origin.Root;
			ImmutableHashSet<T>.HashBucket hashBucket;
			if (origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
			{
				ImmutableHashSet<T>.HashBucket newBucket = hashBucket.Remove(item, origin.EqualityComparer, out operationResult);
				if (operationResult == ImmutableHashSet<T>.OperationResult.NoChangeRequired)
				{
					return new ImmutableHashSet<T>.MutationResult(origin.Root, 0, ImmutableHashSet<T>.CountType.Adjustment);
				}
				node = ImmutableHashSet<T>.UpdateRoot(origin.Root, hashCode, newBucket);
			}
			return new ImmutableHashSet<T>.MutationResult(node, (operationResult == ImmutableHashSet<T>.OperationResult.SizeChanged) ? -1 : 0, ImmutableHashSet<T>.CountType.Adjustment);
		}
		private static bool Contains(T item, ImmutableHashSet<T>.MutationInput origin)
		{
			int hashCode = origin.EqualityComparer.GetHashCode(item);
			ImmutableHashSet<T>.HashBucket hashBucket;
			return origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket) && hashBucket.Contains(item, origin.EqualityComparer);
		}
		private static ImmutableHashSet<T>.MutationResult Union(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			int num = 0;
			ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node node = origin.Root;
			foreach (T current in other)
			{
				int hashCode = origin.EqualityComparer.GetHashCode(current);
				ImmutableHashSet<T>.OperationResult operationResult;
				ImmutableHashSet<T>.HashBucket newBucket = node.GetValueOrDefault(hashCode, Comparer<int>.Default).Add(current, origin.EqualityComparer, out operationResult);
				if (operationResult == ImmutableHashSet<T>.OperationResult.SizeChanged)
				{
					node = ImmutableHashSet<T>.UpdateRoot(node, hashCode, newBucket);
					num++;
				}
			}
			return new ImmutableHashSet<T>.MutationResult(node, num, ImmutableHashSet<T>.CountType.Adjustment);
		}
		private static bool Overlaps(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (origin.Root.IsEmpty)
			{
				return false;
			}
			foreach (T current in other)
			{
				if (ImmutableHashSet<T>.Contains(current, origin))
				{
					return true;
				}
			}
			return false;
		}
		private static bool SetEquals(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
			if (origin.Count != hashSet.Count)
			{
				return false;
			}
			int num = 0;
			foreach (T current in hashSet)
			{
				if (!ImmutableHashSet<T>.Contains(current, origin))
				{
					return false;
				}
				num++;
			}
			return num == origin.Count;
		}
		private static ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node UpdateRoot(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, int hashCode, ImmutableHashSet<T>.HashBucket newBucket)
		{
			bool flag;
			if (newBucket.IsEmpty)
			{
				return root.Remove(hashCode, Comparer<int>.Default, out flag);
			}
			bool flag2;
			return root.SetItem(hashCode, newBucket, Comparer<int>.Default, EqualityComparer<ImmutableHashSet<T>.HashBucket>.Default, out flag2, out flag);
		}
		private static ImmutableHashSet<T>.MutationResult Intersect(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node emptyNode = ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
			int num = 0;
			foreach (T current in other)
			{
				if (ImmutableHashSet<T>.Contains(current, origin))
				{
					ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Add(current, new ImmutableHashSet<T>.MutationInput(emptyNode, origin.EqualityComparer, num));
					emptyNode = mutationResult.Root;
					num += mutationResult.Count;
				}
			}
			return new ImmutableHashSet<T>.MutationResult(emptyNode, num, ImmutableHashSet<T>.CountType.FinalValue);
		}
		private static ImmutableHashSet<T>.MutationResult Except(IEnumerable<T> other, IEqualityComparer<T> equalityComparer, ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
			Requires.NotNull<ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
			int num = 0;
			ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node node = root;
			foreach (T current in other)
			{
				int hashCode = equalityComparer.GetHashCode(current);
				ImmutableHashSet<T>.HashBucket hashBucket;
				if (node.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
				{
					ImmutableHashSet<T>.OperationResult operationResult;
					ImmutableHashSet<T>.HashBucket newBucket = hashBucket.Remove(current, equalityComparer, out operationResult);
					if (operationResult == ImmutableHashSet<T>.OperationResult.SizeChanged)
					{
						num--;
						node = ImmutableHashSet<T>.UpdateRoot(node, hashCode, newBucket);
					}
				}
			}
			return new ImmutableHashSet<T>.MutationResult(node, num, ImmutableHashSet<T>.CountType.Adjustment);
		}
		private static ImmutableHashSet<T>.MutationResult SymmetricExcept(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			ImmutableHashSet<T> immutableHashSet = ImmutableHashSet<T>.Empty.Union(other);
			int num = 0;
			ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node emptyNode = ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
			foreach (T current in new ImmutableHashSet<T>.NodeEnumerable(origin.Root))
			{
				if (!immutableHashSet.Contains(current))
				{
					ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Add(current, new ImmutableHashSet<T>.MutationInput(emptyNode, origin.EqualityComparer, num));
					emptyNode = mutationResult.Root;
					num += mutationResult.Count;
				}
			}
			foreach (T current2 in immutableHashSet)
			{
				if (!ImmutableHashSet<T>.Contains(current2, origin))
				{
					ImmutableHashSet<T>.MutationResult mutationResult2 = ImmutableHashSet<T>.Add(current2, new ImmutableHashSet<T>.MutationInput(emptyNode, origin.EqualityComparer, num));
					emptyNode = mutationResult2.Root;
					num += mutationResult2.Count;
				}
			}
			return new ImmutableHashSet<T>.MutationResult(emptyNode, num, ImmutableHashSet<T>.CountType.FinalValue);
		}
		private static bool IsProperSubsetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (origin.Root.IsEmpty)
			{
				return other.Any<T>();
			}
			HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
			if (origin.Count >= hashSet.Count)
			{
				return false;
			}
			int num = 0;
			bool flag = false;
			foreach (T current in hashSet)
			{
				if (ImmutableHashSet<T>.Contains(current, origin))
				{
					num++;
				}
				else
				{
					flag = true;
				}
				if (num == origin.Count && flag)
				{
					return true;
				}
			}
			return false;
		}
		private static bool IsProperSupersetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (origin.Root.IsEmpty)
			{
				return false;
			}
			int num = 0;
			foreach (T current in other)
			{
				num++;
				if (!ImmutableHashSet<T>.Contains(current, origin))
				{
					return false;
				}
			}
			return origin.Count > num;
		}
		private static bool IsSubsetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (origin.Root.IsEmpty)
			{
				return true;
			}
			HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
			int num = 0;
			foreach (T current in hashSet)
			{
				if (ImmutableHashSet<T>.Contains(current, origin))
				{
					num++;
				}
			}
			return num == origin.Count;
		}
		private static ImmutableHashSet<T> Wrap(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, IEqualityComparer<T> equalityComparer, int count)
		{
			Requires.NotNull<ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
			Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
			Requires.Range(count >= 0, "count", null);
			return new ImmutableHashSet<T>(root, equalityComparer, count);
		}
		private ImmutableHashSet<T> Wrap(ImmutableSortedDictionary<int, ImmutableHashSet<T>.HashBucket>.Node root, int adjustedCountIfDifferentRoot)
		{
			if (root == this.root)
			{
				return this;
			}
			return new ImmutableHashSet<T>(root, this.equalityComparer, adjustedCountIfDifferentRoot);
		}
		private ImmutableHashSet<T> Union(IEnumerable<T> items, bool avoidWithComparer)
		{
			Requires.NotNull<IEnumerable<T>>(items, "items");
			if (this.IsEmpty && !avoidWithComparer)
			{
				ImmutableHashSet<T> immutableHashSet = items as ImmutableHashSet<T>;
				if (immutableHashSet != null)
				{
					return immutableHashSet.WithComparer(this.KeyComparer);
				}
			}
			return ImmutableHashSet<T>.Union(items, this.Origin).Finalize(this);
		}
	}
}
