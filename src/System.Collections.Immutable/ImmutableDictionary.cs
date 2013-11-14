using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Validation;
namespace System.Collections.Immutable
{
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableDictionary<, >.DebuggerProxy))]
	public sealed class ImmutableDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IHashKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		internal class Comparers : IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket>, IEqualityComparer<KeyValuePair<TKey, TValue>>
		{
			internal static readonly ImmutableDictionary<TKey, TValue>.Comparers Default = new ImmutableDictionary<TKey, TValue>.Comparers(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
			private readonly IEqualityComparer<TKey> keyComparer;
			private readonly IEqualityComparer<TValue> valueComparer;
			internal IEqualityComparer<TKey> KeyComparer
			{
				get
				{
					return this.keyComparer;
				}
			}
			internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer
			{
				get
				{
					return this;
				}
			}
			internal IEqualityComparer<TValue> ValueComparer
			{
				get
				{
					return this.valueComparer;
				}
			}
			internal IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket> HashBucketEqualityComparer
			{
				get
				{
					return this;
				}
			}
			internal Comparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
			{
				Requires.NotNull<IEqualityComparer<TKey>>(keyComparer, "keyComparer");
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				this.keyComparer = keyComparer;
				this.valueComparer = valueComparer;
			}
			public bool Equals(ImmutableDictionary<TKey, TValue>.HashBucket x, ImmutableDictionary<TKey, TValue>.HashBucket y)
			{
				return object.ReferenceEquals(x.AdditionalElements, y.AdditionalElements) && this.KeyComparer.Equals(x.FirstValue.Key, y.FirstValue.Key) && this.ValueComparer.Equals(x.FirstValue.Value, y.FirstValue.Value);
			}
			public int GetHashCode(ImmutableDictionary<TKey, TValue>.HashBucket obj)
			{
				return this.KeyComparer.GetHashCode(obj.FirstValue.Key);
			}
			bool IEqualityComparer<KeyValuePair<TKey, TValue>>.Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
			{
				return this.keyComparer.Equals(x.Key, y.Key);
			}
			int IEqualityComparer<KeyValuePair<TKey, TValue>>.GetHashCode(KeyValuePair<TKey, TValue> obj)
			{
				return this.keyComparer.GetHashCode(obj.Key);
			}
			internal static ImmutableDictionary<TKey, TValue>.Comparers Get(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
			{
				Requires.NotNull<IEqualityComparer<TKey>>(keyComparer, "keyComparer");
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				if (keyComparer != ImmutableDictionary<TKey, TValue>.Comparers.Default.KeyComparer || valueComparer != ImmutableDictionary<TKey, TValue>.Comparers.Default.ValueComparer)
				{
					return new ImmutableDictionary<TKey, TValue>.Comparers(keyComparer, valueComparer);
				}
				return ImmutableDictionary<TKey, TValue>.Comparers.Default;
			}
			internal ImmutableDictionary<TKey, TValue>.Comparers WithValueComparer(IEqualityComparer<TValue> valueComparer)
			{
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				if (this.valueComparer != valueComparer)
				{
					return ImmutableDictionary<TKey, TValue>.Comparers.Get(this.KeyComparer, valueComparer);
				}
				return this;
			}
		}
		[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableDictionary<, >.Builder.DebuggerProxy))]
		public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
		{
			[ExcludeFromCodeCoverage]
			private class DebuggerProxy
			{
				private readonly ImmutableDictionary<TKey, TValue>.Builder map;
				private KeyValuePair<TKey, TValue>[] contents;
				[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
				public KeyValuePair<TKey, TValue>[] Contents
				{
					get
					{
						if (this.contents == null)
						{
							this.contents = this.map.ToArray(this.map.Count);
						}
						return this.contents;
					}
				}
				public DebuggerProxy(ImmutableDictionary<TKey, TValue>.Builder map)
				{
					Requires.NotNull<ImmutableDictionary<TKey, TValue>.Builder>(map, "map");
					this.map = map;
				}
			}
			private ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root = ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode;
			private ImmutableDictionary<TKey, TValue>.Comparers comparers;
			private int count;
			private ImmutableDictionary<TKey, TValue> immutable;
			private int version;
			private object syncRoot;
			public IEqualityComparer<TKey> KeyComparer
			{
				get
				{
					return this.comparers.KeyComparer;
				}
				set
				{
					Requires.NotNull<IEqualityComparer<TKey>>(value, "value");
					if (value != this.KeyComparer)
					{
						ImmutableDictionary<TKey, TValue>.Comparers comparers = ImmutableDictionary<TKey, TValue>.Comparers.Get(value, this.ValueComparer);
						ImmutableDictionary<TKey, TValue>.MutationInput origin = new ImmutableDictionary<TKey, TValue>.MutationInput(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode, comparers, 0);
						ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange(this, origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent);
						this.immutable = null;
						this.comparers = comparers;
						this.count = mutationResult.CountAdjustment;
						this.Root = mutationResult.Root;
					}
				}
			}
			public IEqualityComparer<TValue> ValueComparer
			{
				get
				{
					return this.comparers.ValueComparer;
				}
				set
				{
					Requires.NotNull<IEqualityComparer<TValue>>(value, "value");
					if (value != this.ValueComparer)
					{
						this.comparers = this.comparers.WithValueComparer(value);
						this.immutable = null;
					}
				}
			}
			public int Count
			{
				get
				{
					return this.count;
				}
			}
			bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
			{
				get
				{
					return false;
				}
			}
			public IEnumerable<TKey> Keys
			{
				get
				{
					return 
						from kv in this.root.Values.SelectMany((ImmutableDictionary<TKey, TValue>.HashBucket b) => b)
						select kv.Key;
				}
			}
			ICollection<TKey> IDictionary<TKey, TValue>.Keys
			{
				get
				{
					return this.Keys.ToArray(this.Count);
				}
			}
			public IEnumerable<TValue> Values
			{
				get
				{
					return (
						from kv in this.root.Values.SelectMany((ImmutableDictionary<TKey, TValue>.HashBucket b) => b)
						select kv.Value).ToArray(this.Count);
				}
			}
			ICollection<TValue> IDictionary<TKey, TValue>.Values
			{
				get
				{
					return this.Values.ToArray(this.Count);
				}
			}
			bool IDictionary.IsFixedSize
			{
				get
				{
					return false;
				}
			}
			bool IDictionary.IsReadOnly
			{
				get
				{
					return false;
				}
			}
			ICollection IDictionary.Keys
			{
				get
				{
					return this.Keys.ToArray(this.Count);
				}
			}
			ICollection IDictionary.Values
			{
				get
				{
					return this.Values.ToArray(this.Count);
				}
			}
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			object ICollection.SyncRoot
			{
				get
				{
					if (this.syncRoot == null)
					{
						Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
					}
					return this.syncRoot;
				}
			}
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			bool ICollection.IsSynchronized
			{
				get
				{
					return false;
				}
			}
			object IDictionary.this[object key]
			{
				get
				{
					return this[(TKey)((object)key)];
				}
				set
				{
					this[(TKey)((object)key)] = (TValue)((object)value);
				}
			}
			internal int Version
			{
				get
				{
					return this.version;
				}
			}
			private ImmutableDictionary<TKey, TValue>.MutationInput Origin
			{
				get
				{
					return new ImmutableDictionary<TKey, TValue>.MutationInput(this.Root, this.comparers, this.count);
				}
			}
			private ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node Root
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
			public TValue this[TKey key]
			{
				get
				{
					TValue result;
					if (this.TryGetValue(key, out result))
					{
						return result;
					}
					throw new KeyNotFoundException();
				}
				set
				{
					ImmutableDictionary<TKey, TValue>.MutationResult result = ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue, this.Origin);
					this.Apply(result);
				}
			}
			internal Builder(ImmutableDictionary<TKey, TValue> map)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(map, "map");
				this.root = map.root;
				this.count = map.count;
				this.comparers = map.comparers;
				this.immutable = map;
			}
			void IDictionary.Add(object key, object value)
			{
				this.Add((TKey)((object)key), (TValue)((object)value));
			}
			bool IDictionary.Contains(object key)
			{
				return this.ContainsKey((TKey)((object)key));
			}
			IDictionaryEnumerator IDictionary.GetEnumerator()
			{
				return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());
			}
			void IDictionary.Remove(object key)
			{
				this.Remove((TKey)((object)key));
			}
			void ICollection.CopyTo(Array array, int arrayIndex)
			{
				Requires.NotNull<Array>(array, "array");
				Requires.Range(arrayIndex >= 0, "arrayIndex", null);
				Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
				foreach (KeyValuePair<TKey, TValue> current in this)
				{
					array.SetValue(new DictionaryEntry(current.Key, current.Value), new int[]
					{
						arrayIndex++
					});
				}
			}
			public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
			{
				ImmutableDictionary<TKey, TValue>.MutationResult result = ImmutableDictionary<TKey, TValue>.AddRange(items, this.Origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent);
				this.Apply(result);
			}
			public void RemoveRange(IEnumerable<TKey> keys)
			{
				Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
				foreach (TKey current in keys)
				{
					this.Remove(current);
				}
			}
			public ImmutableDictionary<TKey, TValue>.Enumerator GetEnumerator()
			{
				return new ImmutableDictionary<TKey, TValue>.Enumerator(this.root, this);
			}
			public TValue GetValueOrDefault(TKey key)
			{
				return this.GetValueOrDefault(key, default(TValue));
			}
			public TValue GetValueOrDefault(TKey key, TValue defaultValue)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				TValue result;
				if (this.TryGetValue(key, out result))
				{
					return result;
				}
				return defaultValue;
			}
			public ImmutableDictionary<TKey, TValue> ToImmutable()
			{
				if (this.immutable == null)
				{
					this.immutable = ImmutableDictionary<TKey, TValue>.Wrap(this.root, this.comparers, this.count);
				}
				return this.immutable;
			}
			public void Add(TKey key, TValue value)
			{
				ImmutableDictionary<TKey, TValue>.MutationResult result = ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent, this.Origin);
				this.Apply(result);
			}
			public bool ContainsKey(TKey key)
			{
				return ImmutableDictionary<TKey, TValue>.ContainsKey(key, this.Origin);
			}
			public bool ContainsValue(TValue value)
			{
				return this.Values.Contains(value, this.ValueComparer);
			}
			public bool Remove(TKey key)
			{
				ImmutableDictionary<TKey, TValue>.MutationResult result = ImmutableDictionary<TKey, TValue>.Remove(key, this.Origin);
				return this.Apply(result);
			}
			public bool TryGetValue(TKey key, out TValue value)
			{
				return ImmutableDictionary<TKey, TValue>.TryGetValue(key, this.Origin, out value);
			}
			public bool TryGetKey(TKey equalKey, out TKey actualKey)
			{
				return ImmutableDictionary<TKey, TValue>.TryGetKey(equalKey, this.Origin, out actualKey);
			}
			public void Add(KeyValuePair<TKey, TValue> item)
			{
				this.Add(item.Key, item.Value);
			}
			public void Clear()
			{
				this.Root = ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode;
				this.count = 0;
			}
			public bool Contains(KeyValuePair<TKey, TValue> item)
			{
				return ImmutableDictionary<TKey, TValue>.Contains(item, this.Origin);
			}
			void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
				foreach (KeyValuePair<TKey, TValue> current in this)
				{
					array[arrayIndex++] = current;
				}
			}
			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				return this.Contains(item) && this.Remove(item.Key);
			}
			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			private bool Apply(ImmutableDictionary<TKey, TValue>.MutationResult result)
			{
				this.Root = result.Root;
				this.count += result.CountAdjustment;
				return result.CountAdjustment != 0;
			}
		}
		[ExcludeFromCodeCoverage]
		private class DebuggerProxy
		{
			private readonly ImmutableDictionary<TKey, TValue> map;
			private KeyValuePair<TKey, TValue>[] contents;
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public KeyValuePair<TKey, TValue>[] Contents
			{
				get
				{
					if (this.contents == null)
					{
						this.contents = this.map.ToArray(this.map.Count);
					}
					return this.contents;
				}
			}
			public DebuggerProxy(ImmutableDictionary<TKey, TValue> map)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(map, "map");
				this.map = map;
			}
		}
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
		{
			private readonly ImmutableDictionary<TKey, TValue>.Builder builder;
			private ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Enumerator mapEnumerator;
			private ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator bucketEnumerator;
			private int enumeratingBuilderVersion;
			public KeyValuePair<TKey, TValue> Current
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
			internal Enumerator(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Builder builder = null)
			{
				this.builder = builder;
				this.mapEnumerator = new ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Enumerator(root, null);
				this.bucketEnumerator = default(ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator);
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
					KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket> current = this.mapEnumerator.Current;
					this.bucketEnumerator = new ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator(current.Value);
					return this.bucketEnumerator.MoveNext();
				}
				return false;
			}
			public void Reset()
			{
				this.enumeratingBuilderVersion = ((this.builder != null) ? this.builder.Version : -1);
				this.mapEnumerator.Reset();
				this.bucketEnumerator.Dispose();
				this.bucketEnumerator = default(ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator);
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
		internal struct HashBucket : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IEquatable<ImmutableDictionary<TKey, TValue>.HashBucket>
		{
			internal struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
			{
				private enum Position
				{
					BeforeFirst,
					First,
					Additional,
					End
				}
				private readonly ImmutableDictionary<TKey, TValue>.HashBucket bucket;
				private ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position currentPosition;
				private ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator additionalEnumerator;
				object IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}
				public KeyValuePair<TKey, TValue> Current
				{
					get
					{
						switch (this.currentPosition)
						{
						case ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.First:
							return this.bucket.firstValue;
						case ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.Additional:
							return this.additionalEnumerator.Current;
						default:
							throw new InvalidOperationException();
						}
					}
				}
				internal Enumerator(ImmutableDictionary<TKey, TValue>.HashBucket bucket)
				{
					this.bucket = bucket;
					this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.BeforeFirst;
					this.additionalEnumerator = default(ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator);
				}
				public bool MoveNext()
				{
					if (this.bucket.IsEmpty)
					{
						this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.End;
						return false;
					}
					switch (this.currentPosition)
					{
					case ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.BeforeFirst:
						this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.First;
						return true;
					case ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.First:
						if (this.bucket.additionalElements.IsEmpty)
						{
							this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.End;
							return false;
						}
						this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.Additional;
						this.additionalEnumerator = new ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator(this.bucket.additionalElements, null, -1, -1, false);
						return this.additionalEnumerator.MoveNext();
					case ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.Additional:
						return this.additionalEnumerator.MoveNext();
					case ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.End:
						return false;
					default:
						throw new InvalidOperationException();
					}
				}
				public void Reset()
				{
					this.additionalEnumerator.Dispose();
					this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.BeforeFirst;
				}
				public void Dispose()
				{
					this.additionalEnumerator.Dispose();
				}
			}
			private readonly KeyValuePair<TKey, TValue> firstValue;
			private readonly ImmutableList<KeyValuePair<TKey, TValue>>.Node additionalElements;
			internal bool IsEmpty
			{
				get
				{
					return this.additionalElements == null;
				}
			}
			internal KeyValuePair<TKey, TValue> FirstValue
			{
				get
				{
					if (this.IsEmpty)
					{
						throw new InvalidOperationException();
					}
					return this.firstValue;
				}
			}
			internal ImmutableList<KeyValuePair<TKey, TValue>>.Node AdditionalElements
			{
				get
				{
					return this.additionalElements;
				}
			}
			private HashBucket(KeyValuePair<TKey, TValue> firstElement, ImmutableList<KeyValuePair<TKey, TValue>>.Node additionalElements = null)
			{
				this.firstValue = firstElement;
				this.additionalElements = (additionalElements ?? ImmutableList<KeyValuePair<TKey, TValue>>.Node.EmptyNode);
			}
			public ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator GetEnumerator()
			{
				return new ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator(this);
			}
			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			bool IEquatable<ImmutableDictionary<TKey, TValue>.HashBucket>.Equals(ImmutableDictionary<TKey, TValue>.HashBucket other)
			{
				throw new Exception();
			}
			internal ImmutableDictionary<TKey, TValue>.HashBucket Add(TKey key, TValue value, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, IEqualityComparer<TValue> valueComparer, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior behavior, out ImmutableDictionary<TKey, TValue>.OperationResult result)
			{
				KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
				if (this.IsEmpty)
				{
					result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
					return new ImmutableDictionary<TKey, TValue>.HashBucket(keyValuePair, null);
				}
				if (keyOnlyComparer.Equals(keyValuePair, this.firstValue))
				{
					switch (behavior)
					{
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue:
						result = ImmutableDictionary<TKey, TValue>.OperationResult.AppliedWithoutSizeChange;
						return new ImmutableDictionary<TKey, TValue>.HashBucket(keyValuePair, this.additionalElements);
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.Skip:
						result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
						return this;
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent:
						if (!valueComparer.Equals(this.firstValue.Value, value))
						{
							throw new ArgumentException(Strings.DuplicateKey);
						}
						result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
						return this;
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowAlways:
						throw new ArgumentException(Strings.DuplicateKey);
					default:
						throw new InvalidOperationException();
					}
				}
				else
				{
					int num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
					if (num < 0)
					{
						result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
						return new ImmutableDictionary<TKey, TValue>.HashBucket(this.firstValue, this.additionalElements.Add(keyValuePair));
					}
					switch (behavior)
					{
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue:
						result = ImmutableDictionary<TKey, TValue>.OperationResult.AppliedWithoutSizeChange;
						return new ImmutableDictionary<TKey, TValue>.HashBucket(this.firstValue, this.additionalElements.ReplaceAt(num, keyValuePair));
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.Skip:
						result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
						return this;
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent:
						if (!valueComparer.Equals(this.additionalElements[num].Value, value))
						{
							throw new ArgumentException(Strings.DuplicateKey);
						}
						result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
						return this;
					case ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowAlways:
						throw new ArgumentException(Strings.DuplicateKey);
					default:
						throw new InvalidOperationException();
					}
				}
			}
			internal ImmutableDictionary<TKey, TValue>.HashBucket Remove(TKey key, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out ImmutableDictionary<TKey, TValue>.OperationResult result)
			{
				if (this.IsEmpty)
				{
					result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
					return this;
				}
				KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, default(TValue));
				if (keyOnlyComparer.Equals(this.firstValue, keyValuePair))
				{
					if (this.additionalElements.IsEmpty)
					{
						result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
						return default(ImmutableDictionary<TKey, TValue>.HashBucket);
					}
					int count = ((IBinaryTree<KeyValuePair<TKey, TValue>>)this.additionalElements).Left.Count;
					result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
					return new ImmutableDictionary<TKey, TValue>.HashBucket(this.additionalElements.Key, this.additionalElements.RemoveAt(count));
				}
				else
				{
					int num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
					if (num < 0)
					{
						result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
						return this;
					}
					result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
					return new ImmutableDictionary<TKey, TValue>.HashBucket(this.firstValue, this.additionalElements.RemoveAt(num));
				}
			}
			internal bool TryGetValue(TKey key, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out TValue value)
			{
				if (this.IsEmpty)
				{
					value = default(TValue);
					return false;
				}
				KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, default(TValue));
				if (keyOnlyComparer.Equals(this.firstValue, keyValuePair))
				{
					value = this.firstValue.Value;
					return true;
				}
				int num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
				if (num < 0)
				{
					value = default(TValue);
					return false;
				}
				value = this.additionalElements[num].Value;
				return true;
			}
			internal bool TryGetKey(TKey equalKey, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out TKey actualKey)
			{
				if (this.IsEmpty)
				{
					actualKey = equalKey;
					return false;
				}
				KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(equalKey, default(TValue));
				if (keyOnlyComparer.Equals(this.firstValue, keyValuePair))
				{
					actualKey = this.firstValue.Key;
					return true;
				}
				int num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
				if (num < 0)
				{
					actualKey = equalKey;
					return false;
				}
				actualKey = this.additionalElements[num].Key;
				return true;
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
			private readonly ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;
			private readonly ImmutableDictionary<TKey, TValue>.Comparers comparers;
			private readonly int count;
			internal ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node Root
			{
				get
				{
					return this.root;
				}
			}
			internal IEqualityComparer<TKey> KeyComparer
			{
				get
				{
					return this.comparers.KeyComparer;
				}
			}
			internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer
			{
				get
				{
					return this.comparers.KeyOnlyComparer;
				}
			}
			internal IEqualityComparer<TValue> ValueComparer
			{
				get
				{
					return this.comparers.ValueComparer;
				}
			}
			internal IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket> HashBucketComparer
			{
				get
				{
					return this.comparers.HashBucketEqualityComparer;
				}
			}
			internal int Count
			{
				get
				{
					return this.count;
				}
			}
			internal MutationInput(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Comparers comparers, int count)
			{
				this.root = root;
				this.comparers = comparers;
				this.count = count;
			}
			internal MutationInput(ImmutableDictionary<TKey, TValue> map)
			{
				this.root = map.root;
				this.comparers = map.comparers;
				this.count = map.count;
			}
		}
		private struct MutationResult
		{
			private readonly ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;
			private readonly int countAdjustment;
			internal ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node Root
			{
				get
				{
					return this.root;
				}
			}
			internal int CountAdjustment
			{
				get
				{
					return this.countAdjustment;
				}
			}
			internal MutationResult(ImmutableDictionary<TKey, TValue>.MutationInput unchangedInput)
			{
				this.root = unchangedInput.Root;
				this.countAdjustment = 0;
			}
			internal MutationResult(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, int countAdjustment)
			{
				Requires.NotNull<ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node>(root, "root");
				this.root = root;
				this.countAdjustment = countAdjustment;
			}
			internal ImmutableDictionary<TKey, TValue> Finalize(ImmutableDictionary<TKey, TValue> priorMap)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(priorMap, "priorMap");
				return priorMap.Wrap(this.Root, priorMap.count + this.CountAdjustment);
			}
		}
		internal enum KeyCollisionBehavior
		{
			SetValue,
			Skip,
			ThrowIfValueDifferent,
			ThrowAlways
		}
		internal enum OperationResult
		{
			AppliedWithoutSizeChange,
			SizeChanged,
			NoChangeRequired
		}
		public static readonly ImmutableDictionary<TKey, TValue> Empty = new ImmutableDictionary<TKey, TValue>(null);
		private static readonly Action<KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket>> FreezeBucketAction = delegate(KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket> kv)
		{
			kv.Value.Freeze();
		};
		private readonly int count;
		private readonly ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;
		private readonly ImmutableDictionary<TKey, TValue>.Comparers comparers;
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
		public IEqualityComparer<TKey> KeyComparer
		{
			get
			{
				return this.comparers.KeyComparer;
			}
		}
		public IEqualityComparer<TValue> ValueComparer
		{
			get
			{
				return this.comparers.ValueComparer;
			}
		}
		public IEnumerable<TKey> Keys
		{
			get
			{
				foreach (KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket> current in this.root)
				{
					KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket> keyValuePair = current;
					foreach (KeyValuePair<TKey, TValue> current2 in keyValuePair.Value)
					{
						KeyValuePair<TKey, TValue> keyValuePair2 = current2;
						yield return keyValuePair2.Key;
					}
				}
				yield break;
			}
		}
		public IEnumerable<TValue> Values
		{
			get
			{
				foreach (KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket> current in this.root)
				{
					KeyValuePair<int, ImmutableDictionary<TKey, TValue>.HashBucket> keyValuePair = current;
					foreach (KeyValuePair<TKey, TValue> current2 in keyValuePair.Value)
					{
						KeyValuePair<TKey, TValue> keyValuePair2 = current2;
						yield return keyValuePair2.Value;
					}
				}
				yield break;
			}
		}
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				return new KeysCollectionAccessor<TKey, TValue>(this);
			}
		}
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				return new ValuesCollectionAccessor<TKey, TValue>(this);
			}
		}
		private ImmutableDictionary<TKey, TValue>.MutationInput Origin
		{
			get
			{
				return new ImmutableDictionary<TKey, TValue>.MutationInput(this);
			}
		}
		public TValue this[TKey key]
		{
			get
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				TValue result;
				if (this.TryGetValue(key, out result))
				{
					return result;
				}
				throw new KeyNotFoundException();
			}
		}
		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get
			{
				return this[key];
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get
			{
				return true;
			}
		}
		bool IDictionary.IsFixedSize
		{
			get
			{
				return true;
			}
		}
		bool IDictionary.IsReadOnly
		{
			get
			{
				return true;
			}
		}
		ICollection IDictionary.Keys
		{
			get
			{
				return new KeysCollectionAccessor<TKey, TValue>(this);
			}
		}
		ICollection IDictionary.Values
		{
			get
			{
				return new ValuesCollectionAccessor<TKey, TValue>(this);
			}
		}
		object IDictionary.this[object key]
		{
			get
			{
				return this[(TKey)((object)key)];
			}
			set
			{
				throw new NotSupportedException();
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
		private ImmutableDictionary(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Comparers comparers, int count) : this(Requires.NotNull<ImmutableDictionary<TKey, TValue>.Comparers>(comparers, "comparers"))
		{
			Requires.NotNull<ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node>(root, "root");
			root.Freeze(ImmutableDictionary<TKey, TValue>.FreezeBucketAction);
			this.root = root;
			this.count = count;
		}
		private ImmutableDictionary(ImmutableDictionary<TKey, TValue>.Comparers comparers = null)
		{
			this.comparers = (comparers ?? ImmutableDictionary<TKey, TValue>.Comparers.Get(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default));
			this.root = ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode;
		}
		public ImmutableDictionary<TKey, TValue> Clear()
		{
			if (!this.IsEmpty)
			{
				return ImmutableDictionary<TKey, TValue>.EmptyWithComparers(this.comparers);
			}
			return this;
		}
		[ExcludeFromCodeCoverage]
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
		{
			return this.Clear();
		}
		public ImmutableDictionary<TKey, TValue>.Builder ToBuilder()
		{
			return new ImmutableDictionary<TKey, TValue>.Builder(this);
		}
		public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent, new ImmutableDictionary<TKey, TValue>.MutationInput(this)).Finalize(this);
		}
		public ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(pairs, "pairs");
			return this.AddRange(pairs, false);
		}
		public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue, new ImmutableDictionary<TKey, TValue>.MutationInput(this)).Finalize(this);
		}
		public ImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
			return ImmutableDictionary<TKey, TValue>.AddRange(items, this.Origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue).Finalize(this);
		}
		public ImmutableDictionary<TKey, TValue> Remove(TKey key)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return ImmutableDictionary<TKey, TValue>.Remove(key, new ImmutableDictionary<TKey, TValue>.MutationInput(this)).Finalize(this);
		}
		public ImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
		{
			Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
			int num = this.count;
			ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node node = this.root;
			foreach (TKey current in keys)
			{
				int hashCode = this.KeyComparer.GetHashCode(current);
				ImmutableDictionary<TKey, TValue>.HashBucket hashBucket;
				if (node.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
				{
					ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
					ImmutableDictionary<TKey, TValue>.HashBucket newBucket = hashBucket.Remove(current, this.comparers.KeyOnlyComparer, out operationResult);
					node = ImmutableDictionary<TKey, TValue>.UpdateRoot(node, hashCode, newBucket, this.comparers.HashBucketEqualityComparer);
					if (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged)
					{
						num--;
					}
				}
			}
			return this.Wrap(node, num);
		}
		public bool ContainsKey(TKey key)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return ImmutableDictionary<TKey, TValue>.ContainsKey(key, new ImmutableDictionary<TKey, TValue>.MutationInput(this));
		}
		public bool Contains(KeyValuePair<TKey, TValue> pair)
		{
			return ImmutableDictionary<TKey, TValue>.Contains(pair, this.Origin);
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return ImmutableDictionary<TKey, TValue>.TryGetValue(key, this.Origin, out value);
		}
		public bool TryGetKey(TKey equalKey, out TKey actualKey)
		{
			Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
			return ImmutableDictionary<TKey, TValue>.TryGetKey(equalKey, this.Origin, out actualKey);
		}
		public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			if (keyComparer == null)
			{
				keyComparer = EqualityComparer<TKey>.Default;
			}
			if (valueComparer == null)
			{
				valueComparer = EqualityComparer<TValue>.Default;
			}
			if (this.KeyComparer != keyComparer)
			{
				ImmutableDictionary<TKey, TValue>.Comparers comparers = ImmutableDictionary<TKey, TValue>.Comparers.Get(keyComparer, valueComparer);
				ImmutableDictionary<TKey, TValue> immutableDictionary = new ImmutableDictionary<TKey, TValue>(comparers);
				return immutableDictionary.AddRange(this, true);
			}
			if (this.ValueComparer == valueComparer)
			{
				return this;
			}
			ImmutableDictionary<TKey, TValue>.Comparers comparers2 = this.comparers.WithValueComparer(valueComparer);
			return new ImmutableDictionary<TKey, TValue>(this.root, comparers2, this.count);
		}
		public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer)
		{
			return this.WithComparers(keyComparer, this.comparers.ValueComparer);
		}
		public bool ContainsValue(TValue value)
		{
			return this.Values.Contains(value, this.ValueComparer);
		}
		public ImmutableDictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
			return new ImmutableDictionary<TKey, TValue>.Enumerator(this.root, null);
		}
		[ExcludeFromCodeCoverage]
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			return this.Add(key, value);
		}
		[ExcludeFromCodeCoverage]
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
		{
			return this.SetItem(key, value);
		}
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return this.SetItems(items);
		}
		[ExcludeFromCodeCoverage]
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
		{
			return this.AddRange(pairs);
		}
		[ExcludeFromCodeCoverage]
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
		{
			return this.RemoveRange(keys);
		}
		[ExcludeFromCodeCoverage]
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
		{
			return this.Remove(key);
		}
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			throw new NotSupportedException();
		}
		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			throw new NotSupportedException();
		}
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException();
		}
		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			throw new NotSupportedException();
		}
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException();
		}
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex", null);
			Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
			foreach (KeyValuePair<TKey, TValue> current in this)
			{
				array[arrayIndex++] = current;
			}
		}
		void IDictionary.Add(object key, object value)
		{
			throw new NotSupportedException();
		}
		bool IDictionary.Contains(object key)
		{
			return this.ContainsKey((TKey)((object)key));
		}
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());
		}
		void IDictionary.Remove(object key)
		{
			throw new NotSupportedException();
		}
		void IDictionary.Clear()
		{
			throw new NotSupportedException();
		}
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			Requires.NotNull<Array>(array, "array");
			Requires.Range(arrayIndex >= 0, "arrayIndex", null);
			Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
			foreach (KeyValuePair<TKey, TValue> current in this)
			{
				array.SetValue(new DictionaryEntry(current.Key, current.Value), new int[]
				{
					arrayIndex++
				});
			}
		}
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		[ExcludeFromCodeCoverage]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		private static ImmutableDictionary<TKey, TValue> EmptyWithComparers(ImmutableDictionary<TKey, TValue>.Comparers comparers)
		{
			Requires.NotNull<ImmutableDictionary<TKey, TValue>.Comparers>(comparers, "comparers");
			if (ImmutableDictionary<TKey, TValue>.Empty.comparers != comparers)
			{
				return new ImmutableDictionary<TKey, TValue>(comparers);
			}
			return ImmutableDictionary<TKey, TValue>.Empty;
		}
		private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, out ImmutableDictionary<TKey, TValue> other)
		{
			other = (sequence as ImmutableDictionary<TKey, TValue>);
			if (other != null)
			{
				return true;
			}
			ImmutableDictionary<TKey, TValue>.Builder builder = sequence as ImmutableDictionary<TKey, TValue>.Builder;
			if (builder != null)
			{
				other = builder.ToImmutable();
				return true;
			}
			return false;
		}
		private static bool ContainsKey(TKey key, ImmutableDictionary<TKey, TValue>.MutationInput origin)
		{
			int hashCode = origin.KeyComparer.GetHashCode(key);
			ImmutableDictionary<TKey, TValue>.HashBucket hashBucket;
			TValue tValue;
			return origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket) && hashBucket.TryGetValue(key, origin.KeyOnlyComparer, out tValue);
		}
		private static bool Contains(KeyValuePair<TKey, TValue> keyValuePair, ImmutableDictionary<TKey, TValue>.MutationInput origin)
		{
			int hashCode = origin.KeyComparer.GetHashCode(keyValuePair.Key);
			ImmutableDictionary<TKey, TValue>.HashBucket hashBucket;
			TValue x;
			return origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket) && hashBucket.TryGetValue(keyValuePair.Key, origin.KeyOnlyComparer, out x) && origin.ValueComparer.Equals(x, keyValuePair.Value);
		}
		private static bool TryGetValue(TKey key, ImmutableDictionary<TKey, TValue>.MutationInput origin, out TValue value)
		{
			int hashCode = origin.KeyComparer.GetHashCode(key);
			ImmutableDictionary<TKey, TValue>.HashBucket hashBucket;
			if (origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
			{
				return hashBucket.TryGetValue(key, origin.KeyOnlyComparer, out value);
			}
			value = default(TValue);
			return false;
		}
		private static bool TryGetKey(TKey equalKey, ImmutableDictionary<TKey, TValue>.MutationInput origin, out TKey actualKey)
		{
			int hashCode = origin.KeyComparer.GetHashCode(equalKey);
			ImmutableDictionary<TKey, TValue>.HashBucket hashBucket;
			if (origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
			{
				return hashBucket.TryGetKey(equalKey, origin.KeyOnlyComparer, out actualKey);
			}
			actualKey = equalKey;
			return false;
		}
		private static ImmutableDictionary<TKey, TValue>.MutationResult Add(TKey key, TValue value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior behavior, ImmutableDictionary<TKey, TValue>.MutationInput origin)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			int hashCode = origin.KeyComparer.GetHashCode(key);
			ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
			ImmutableDictionary<TKey, TValue>.HashBucket newBucket = origin.Root.GetValueOrDefault(hashCode, Comparer<int>.Default).Add(key, value, origin.KeyOnlyComparer, origin.ValueComparer, behavior, out operationResult);
			if (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired)
			{
				return new ImmutableDictionary<TKey, TValue>.MutationResult(origin);
			}
			ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node node = ImmutableDictionary<TKey, TValue>.UpdateRoot(origin.Root, hashCode, newBucket, origin.HashBucketComparer);
			return new ImmutableDictionary<TKey, TValue>.MutationResult(node, (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged) ? 1 : 0);
		}
		private static ImmutableDictionary<TKey, TValue>.MutationResult AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, ImmutableDictionary<TKey, TValue>.MutationInput origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior collisionBehavior = ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
			int num = 0;
			ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node node = origin.Root;
			foreach (KeyValuePair<TKey, TValue> current in items)
			{
				int hashCode = origin.KeyComparer.GetHashCode(current.Key);
				ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
				ImmutableDictionary<TKey, TValue>.HashBucket newBucket = node.GetValueOrDefault(hashCode, Comparer<int>.Default).Add(current.Key, current.Value, origin.KeyOnlyComparer, origin.ValueComparer, collisionBehavior, out operationResult);
				node = ImmutableDictionary<TKey, TValue>.UpdateRoot(node, hashCode, newBucket, origin.HashBucketComparer);
				if (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged)
				{
					num++;
				}
			}
			return new ImmutableDictionary<TKey, TValue>.MutationResult(node, num);
		}
		private static ImmutableDictionary<TKey, TValue>.MutationResult Remove(TKey key, ImmutableDictionary<TKey, TValue>.MutationInput origin)
		{
			int hashCode = origin.KeyComparer.GetHashCode(key);
			ImmutableDictionary<TKey, TValue>.HashBucket hashBucket;
			if (origin.Root.TryGetValue(hashCode, Comparer<int>.Default, out hashBucket))
			{
				ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
				ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node node = ImmutableDictionary<TKey, TValue>.UpdateRoot(origin.Root, hashCode, hashBucket.Remove(key, origin.KeyOnlyComparer, out operationResult), origin.HashBucketComparer);
				return new ImmutableDictionary<TKey, TValue>.MutationResult(node, (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged) ? -1 : 0);
			}
			return new ImmutableDictionary<TKey, TValue>.MutationResult(origin);
		}
		private static ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node UpdateRoot(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, int hashCode, ImmutableDictionary<TKey, TValue>.HashBucket newBucket, IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket> hashBucketComparer)
		{
			bool flag;
			if (newBucket.IsEmpty)
			{
				return root.Remove(hashCode, Comparer<int>.Default, out flag);
			}
			bool flag2;
			return root.SetItem(hashCode, newBucket, Comparer<int>.Default, hashBucketComparer, out flag2, out flag);
		}
		private static ImmutableDictionary<TKey, TValue> Wrap(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Comparers comparers, int count)
		{
			Requires.NotNull<ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node>(root, "root");
			Requires.NotNull<ImmutableDictionary<TKey, TValue>.Comparers>(comparers, "comparers");
			Requires.Range(count >= 0, "count", null);
			return new ImmutableDictionary<TKey, TValue>(root, comparers, count);
		}
		private ImmutableDictionary<TKey, TValue> Wrap(ImmutableSortedDictionary<int, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, int adjustedCountIfDifferentRoot)
		{
			if (root == null)
			{
				return this.Clear();
			}
			if (this.root == root)
			{
				return this;
			}
			if (!root.IsEmpty)
			{
				return new ImmutableDictionary<TKey, TValue>(root, this.comparers, adjustedCountIfDifferentRoot);
			}
			return this.Clear();
		}
		private ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs, bool avoidToHashMap)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(pairs, "pairs");
			ImmutableDictionary<TKey, TValue> immutableDictionary;
			if (this.IsEmpty && !avoidToHashMap && ImmutableDictionary<TKey, TValue>.TryCastToImmutableMap(pairs, out immutableDictionary))
			{
				return immutableDictionary.WithComparers(this.KeyComparer, this.ValueComparer);
			}
			return ImmutableDictionary<TKey, TValue>.AddRange(pairs, this.Origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent).Finalize(this);
		}
	}
	public static class ImmutableDictionary
	{
		public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>()
		{
			return ImmutableDictionary<TKey, TValue>.Empty;
		}
		public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
		{
			return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
		}
		public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
		}
		public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return ImmutableDictionary<TKey, TValue>.Empty.AddRange(items);
		}
		public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
		}
		public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
		}
		public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>()
		{
			return ImmutableDictionary.Create<TKey, TValue>().ToBuilder();
		}
		public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer)
		{
			return ImmutableDictionary.Create<TKey, TValue>(keyComparer).ToBuilder();
		}
		public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			return ImmutableDictionary.Create<TKey, TValue>(keyComparer, valueComparer).ToBuilder();
		}
		public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull<IEnumerable<TSource>>(source, "source");
			Requires.NotNull<Func<TSource, TKey>>(keySelector, "keySelector");
			Requires.NotNull<Func<TSource, TValue>>(elementSelector, "elementSelector");
			return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(
				from element in source
				select new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element)));
		}
		public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey> keyComparer)
		{
			return source.ToImmutableDictionary(keySelector, elementSelector, keyComparer, null);
		}
		public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return source.ToImmutableDictionary(keySelector, (TSource v) => v, null, null);
		}
		public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
		{
			return source.ToImmutableDictionary(keySelector, (TSource v) => v, keyComparer, null);
		}
		public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
		{
			return source.ToImmutableDictionary(keySelector, elementSelector, null, null);
		}
		public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(source, "source");
			ImmutableDictionary<TKey, TValue> immutableDictionary = source as ImmutableDictionary<TKey, TValue>;
			if (immutableDictionary != null)
			{
				return immutableDictionary.WithComparers(keyComparer, valueComparer);
			}
			return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
		}
		public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> keyComparer)
		{
			return source.ToImmutableDictionary(keyComparer, null);
		}
		public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			return source.ToImmutableDictionary(null, null);
		}
		public static bool Contains<TKey, TValue>(this IImmutableDictionary<TKey, TValue> map, TKey key, TValue value)
		{
			Requires.NotNull<IImmutableDictionary<TKey, TValue>>(map, "map");
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return map.Contains(new KeyValuePair<TKey, TValue>(key, value));
		}
		public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary.GetValueOrDefault(key, default(TValue));
		}
		public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			Requires.NotNull<IImmutableDictionary<TKey, TValue>>(dictionary, "dictionary");
			Requires.NotNullAllowStructs<TKey>(key, "key");
			TValue result;
			if (dictionary.TryGetValue(key, out result))
			{
				return result;
			}
			return defaultValue;
		}
	}
}
