using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Validation;
namespace System.Collections.Immutable
{
	public static class ImmutableSortedDictionary
	{
		public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>()
		{
			return ImmutableSortedDictionary<TKey, TValue>.Empty;
		}
		public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>(IComparer<TKey> keyComparer)
		{
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
		}
		public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
		}
		public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return ImmutableSortedDictionary<TKey, TValue>.Empty.AddRange(items);
		}
		public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
		}
		public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
		}
		public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>()
		{
			return ImmutableSortedDictionary.Create<TKey, TValue>().ToBuilder();
		}
		public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IComparer<TKey> keyComparer)
		{
			return ImmutableSortedDictionary.Create<TKey, TValue>(keyComparer).ToBuilder();
		}
		public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			return ImmutableSortedDictionary.Create<TKey, TValue>(keyComparer, valueComparer).ToBuilder();
		}
		public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull<IEnumerable<TSource>>(source, "source");
			Requires.NotNull<Func<TSource, TKey>>(keySelector, "keySelector");
			Requires.NotNull<Func<TSource, TValue>>(elementSelector, "elementSelector");
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(
				from element in source
				select new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element)));
		}
		public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IComparer<TKey> keyComparer)
		{
			return source.ToImmutableSortedDictionary(keySelector, elementSelector, keyComparer, null);
		}
		public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
		{
			return source.ToImmutableSortedDictionary(keySelector, elementSelector, null, null);
		}
		public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(source, "source");
			ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary = source as ImmutableSortedDictionary<TKey, TValue>;
			if (immutableSortedDictionary != null)
			{
				return immutableSortedDictionary.WithComparers(keyComparer, valueComparer);
			}
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
		}
		public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IComparer<TKey> keyComparer)
		{
			return source.ToImmutableSortedDictionary(keyComparer, null);
		}
		public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
		{
			return source.ToImmutableSortedDictionary(null, null);
		}
	}
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableSortedDictionary<, >.DebuggerProxy))]
	public sealed class ImmutableSortedDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISortKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable, ISecurePooledObjectUser
		{
			private static readonly SecureObjectPool<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>, ImmutableSortedDictionary<TKey, TValue>.Enumerator> enumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>, ImmutableSortedDictionary<TKey, TValue>.Enumerator>();
			private readonly ImmutableSortedDictionary<TKey, TValue>.Builder builder;
			private readonly Guid poolUserId;
			private IBinaryTree<KeyValuePair<TKey, TValue>> root;
			private SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>> stack;
			private IBinaryTree<KeyValuePair<TKey, TValue>> current;
			private int enumeratingBuilderVersion;
			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					this.ThrowIfDisposed();
					if (this.current != null)
					{
						return this.current.Value;
					}
					throw new InvalidOperationException();
				}
			}
			Guid ISecurePooledObjectUser.PoolUserId
			{
				get
				{
					return this.poolUserId;
				}
			}
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			internal Enumerator(IBinaryTree<KeyValuePair<TKey, TValue>> root, ImmutableSortedDictionary<TKey, TValue>.Builder builder = null)
			{
				Requires.NotNull<IBinaryTree<KeyValuePair<TKey, TValue>>>(root, "root");
				this.root = root;
				this.builder = builder;
				this.current = null;
				this.enumeratingBuilderVersion = ((builder != null) ? builder.Version : -1);
				this.poolUserId = Guid.NewGuid();
				this.stack = null;
				if (!ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks.TryTake(this, out this.stack))
				{
					this.stack = ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks.PrepNew(this, new Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>(root.Height));
				}
				this.Reset();
			}
			public void Dispose()
			{
				this.root = null;
				this.current = null;
				if (this.stack != null && this.stack.Owner == this.poolUserId)
				{
					using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this))
					{
						securePooledObjectUser.Value.Clear();
					}
					ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks.TryAdd(this, this.stack);
				}
				this.stack = null;
			}
			public bool MoveNext()
			{
				this.ThrowIfDisposed();
				this.ThrowIfChanged();
				bool result;
				using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this))
				{
					if (securePooledObjectUser.Value.Count > 0)
					{
						IBinaryTree<KeyValuePair<TKey, TValue>> value = securePooledObjectUser.Value.Pop().Value;
						this.current = value;
						this.PushLeft(value.Right);
						result = true;
					}
					else
					{
						this.current = null;
						result = false;
					}
				}
				return result;
			}
			public void Reset()
			{
				this.ThrowIfDisposed();
				this.enumeratingBuilderVersion = ((this.builder != null) ? this.builder.Version : -1);
				this.current = null;
				using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this))
				{
					securePooledObjectUser.Value.Clear();
				}
				this.PushLeft(this.root);
			}
			internal void ThrowIfDisposed()
			{
				if (this.root == null)
				{
					throw new ObjectDisposedException(base.GetType().FullName);
				}
				if (this.stack != null)
				{
					this.stack.ThrowDisposedIfNotOwned<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this);
				}
			}
			private void ThrowIfChanged()
			{
				if (this.builder != null && this.builder.Version != this.enumeratingBuilderVersion)
				{
					throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
				}
			}
			private void PushLeft(IBinaryTree<KeyValuePair<TKey, TValue>> node)
			{
				Requires.NotNull<IBinaryTree<KeyValuePair<TKey, TValue>>>(node, "node");
				using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this))
				{
					while (!node.IsEmpty)
					{
						securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>(node));
						node = node.Left;
					}
				}
			}
		}
		[DebuggerDisplay("{key} = {value}")]
		internal sealed class Node : IBinaryTree<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
		{
			internal static readonly ImmutableSortedDictionary<TKey, TValue>.Node EmptyNode = new ImmutableSortedDictionary<TKey, TValue>.Node();
			private readonly TKey key;
			private TValue value;
			private bool frozen;
			private int height;
			private ImmutableSortedDictionary<TKey, TValue>.Node left;
			private ImmutableSortedDictionary<TKey, TValue>.Node right;
			public bool IsEmpty
			{
				get
				{
					return this.left == null;
				}
			}
			IBinaryTree<KeyValuePair<TKey, TValue>> IBinaryTree<KeyValuePair<TKey, TValue>>.Left
			{
				get
				{
					return this.left;
				}
			}
			IBinaryTree<KeyValuePair<TKey, TValue>> IBinaryTree<KeyValuePair<TKey, TValue>>.Right
			{
				get
				{
					return this.right;
				}
			}
			int IBinaryTree<KeyValuePair<TKey, TValue>>.Height
			{
				get
				{
					return this.height;
				}
			}
			KeyValuePair<TKey, TValue> IBinaryTree<KeyValuePair<TKey, TValue>>.Value
			{
				get
				{
					return new KeyValuePair<TKey, TValue>(this.key, this.value);
				}
			}
			int IBinaryTree<KeyValuePair<TKey, TValue>>.Count
			{
				get
				{
					throw new NotSupportedException();
				}
			}
			internal IEnumerable<TKey> Keys
			{
				get
				{
					return 
						from p in this
						select p.Key;
				}
			}
			internal IEnumerable<TValue> Values
			{
				get
				{
					return 
						from p in this
						select p.Value;
				}
			}
			private Node()
			{
				this.frozen = true;
			}
			private Node(TKey key, TValue value, ImmutableSortedDictionary<TKey, TValue>.Node left, ImmutableSortedDictionary<TKey, TValue>.Node right, bool frozen = false)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(left, "left");
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(right, "right");
				this.key = key;
				this.value = value;
				this.left = left;
				this.right = right;
				this.height = 1 + Math.Max(left.height, right.height);
				this.frozen = frozen;
			}
			public ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
			{
				return new ImmutableSortedDictionary<TKey, TValue>.Enumerator(this, null);
			}
			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			internal ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator(ImmutableSortedDictionary<TKey, TValue>.Builder builder)
			{
				return new ImmutableSortedDictionary<TKey, TValue>.Enumerator(this, builder);
			}
			internal void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex, int dictionarySize)
			{
				Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
				Requires.Range(arrayIndex >= 0, "arrayIndex", null);
				Requires.Range(array.Length >= arrayIndex + dictionarySize, "arrayIndex", null);
				foreach (KeyValuePair<TKey, TValue> current in this)
				{
					array[arrayIndex++] = current;
				}
			}
			internal void CopyTo(Array array, int arrayIndex, int dictionarySize)
			{
				Requires.NotNull<Array>(array, "array");
				Requires.Range(arrayIndex >= 0, "arrayIndex", null);
				Requires.Range(array.Length >= arrayIndex + dictionarySize, "arrayIndex", null);
				foreach (KeyValuePair<TKey, TValue> current in this)
				{
					array.SetValue(new DictionaryEntry(current.Key, current.Value), new int[]
					{
						arrayIndex++
					});
				}
			}
			internal static ImmutableSortedDictionary<TKey, TValue>.Node NodeTreeFromSortedDictionary(SortedDictionary<TKey, TValue> dictionary)
			{
				Requires.NotNull<SortedDictionary<TKey, TValue>>(dictionary, "dictionary");
				IOrderedCollection<KeyValuePair<TKey, TValue>> orderedCollection = dictionary.AsOrderedCollection<KeyValuePair<TKey, TValue>>();
				return ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromList(orderedCollection, 0, orderedCollection.Count);
			}
			internal ImmutableSortedDictionary<TKey, TValue>.Node Add(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, out bool mutated)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				bool flag;
				return this.SetOrAdd(key, value, keyComparer, valueComparer, false, out flag, out mutated);
			}
			internal ImmutableSortedDictionary<TKey, TValue>.Node SetItem(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, out bool replacedExistingValue, out bool mutated)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				return this.SetOrAdd(key, value, keyComparer, valueComparer, true, out replacedExistingValue, out mutated);
			}
			internal ImmutableSortedDictionary<TKey, TValue>.Node Remove(TKey key, IComparer<TKey> keyComparer, out bool mutated)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				return this.RemoveRecursive(key, keyComparer, out mutated);
			}
			internal TValue GetValueOrDefault(TKey key, IComparer<TKey> keyComparer)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(key, keyComparer);
				if (!node.IsEmpty)
				{
					return node.value;
				}
				return default(TValue);
			}
			internal bool TryGetValue(TKey key, IComparer<TKey> keyComparer, out TValue value)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(key, keyComparer);
				if (node.IsEmpty)
				{
					value = default(TValue);
					return false;
				}
				value = node.value;
				return true;
			}
			internal bool TryGetKey(TKey equalKey, IComparer<TKey> keyComparer, out TKey actualKey)
			{
				Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(equalKey, keyComparer);
				if (node.IsEmpty)
				{
					actualKey = equalKey;
					return false;
				}
				actualKey = node.key;
				return true;
			}
			internal bool ContainsKey(TKey key, IComparer<TKey> keyComparer)
			{
				Requires.NotNullAllowStructs<TKey>(key, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				return !this.Search(key, keyComparer).IsEmpty;
			}
			internal bool ContainsValue(TValue value, IEqualityComparer<TValue> valueComparer)
			{
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				return this.Values.Contains(value, valueComparer);
			}
			internal bool Contains(KeyValuePair<TKey, TValue> pair, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
			{
				Requires.NotNullAllowStructs<bool>(pair.Key != null, "key");
				Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
				Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
				ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(pair.Key, keyComparer);
				return !node.IsEmpty && valueComparer.Equals(node.value, pair.Value);
			}
			internal void Freeze(Action<KeyValuePair<TKey, TValue>> freezeAction = null)
			{
				if (!this.frozen)
				{
					if (freezeAction != null)
					{
						freezeAction(new KeyValuePair<TKey, TValue>(this.key, this.value));
					}
					this.left.Freeze(freezeAction);
					this.right.Freeze(freezeAction);
					this.frozen = true;
				}
			}
			private static ImmutableSortedDictionary<TKey, TValue>.Node RotateLeft(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				if (tree.right.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedDictionary<TKey, TValue>.Node node = tree.right;
				return node.Mutate(tree.Mutate(null, node.left), null);
			}
			private static ImmutableSortedDictionary<TKey, TValue>.Node RotateRight(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				if (tree.left.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedDictionary<TKey, TValue>.Node node = tree.left;
				return node.Mutate(null, tree.Mutate(node.right, null));
			}
			private static ImmutableSortedDictionary<TKey, TValue>.Node DoubleLeft(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				if (tree.right.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedDictionary<TKey, TValue>.Node tree2 = tree.Mutate(null, ImmutableSortedDictionary<TKey, TValue>.Node.RotateRight(tree.right));
				return ImmutableSortedDictionary<TKey, TValue>.Node.RotateLeft(tree2);
			}
			private static ImmutableSortedDictionary<TKey, TValue>.Node DoubleRight(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				if (tree.left.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedDictionary<TKey, TValue>.Node tree2 = tree.Mutate(ImmutableSortedDictionary<TKey, TValue>.Node.RotateLeft(tree.left), null);
				return ImmutableSortedDictionary<TKey, TValue>.Node.RotateRight(tree2);
			}
			private static int Balance(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				return tree.right.height - tree.left.height;
			}
			private static bool IsRightHeavy(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				return ImmutableSortedDictionary<TKey, TValue>.Node.Balance(tree) >= 2;
			}
			private static bool IsLeftHeavy(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				return ImmutableSortedDictionary<TKey, TValue>.Node.Balance(tree) <= -2;
			}
			private static ImmutableSortedDictionary<TKey, TValue>.Node MakeBalanced(ImmutableSortedDictionary<TKey, TValue>.Node tree)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
				if (ImmutableSortedDictionary<TKey, TValue>.Node.IsRightHeavy(tree))
				{
					if (!ImmutableSortedDictionary<TKey, TValue>.Node.IsLeftHeavy(tree.right))
					{
						return ImmutableSortedDictionary<TKey, TValue>.Node.RotateLeft(tree);
					}
					return ImmutableSortedDictionary<TKey, TValue>.Node.DoubleLeft(tree);
				}
				else
				{
					if (!ImmutableSortedDictionary<TKey, TValue>.Node.IsLeftHeavy(tree))
					{
						return tree;
					}
					if (!ImmutableSortedDictionary<TKey, TValue>.Node.IsRightHeavy(tree.left))
					{
						return ImmutableSortedDictionary<TKey, TValue>.Node.RotateRight(tree);
					}
					return ImmutableSortedDictionary<TKey, TValue>.Node.DoubleRight(tree);
				}
			}
			private static ImmutableSortedDictionary<TKey, TValue>.Node NodeTreeFromList(IOrderedCollection<KeyValuePair<TKey, TValue>> items, int start, int length)
			{
				Requires.NotNull<IOrderedCollection<KeyValuePair<TKey, TValue>>>(items, "items");
				Requires.Range(start >= 0, "start", null);
				Requires.Range(length >= 0, "length", null);
				if (length == 0)
				{
					return ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
				}
				int num = (length - 1) / 2;
				int num2 = length - 1 - num;
				ImmutableSortedDictionary<TKey, TValue>.Node node = ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromList(items, start, num2);
				ImmutableSortedDictionary<TKey, TValue>.Node node2 = ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromList(items, start + num2 + 1, num);
				KeyValuePair<TKey, TValue> keyValuePair = items[start + num2];
				return new ImmutableSortedDictionary<TKey, TValue>.Node(keyValuePair.Key, keyValuePair.Value, node, node2, true);
			}
			private ImmutableSortedDictionary<TKey, TValue>.Node SetOrAdd(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue, out bool replacedExistingValue, out bool mutated)
			{
				replacedExistingValue = false;
				if (this.IsEmpty)
				{
					mutated = true;
					return new ImmutableSortedDictionary<TKey, TValue>.Node(key, value, this, this, false);
				}
				ImmutableSortedDictionary<TKey, TValue>.Node node = this;
				int num = keyComparer.Compare(key, this.key);
				if (num > 0)
				{
					ImmutableSortedDictionary<TKey, TValue>.Node node2 = this.right.SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
					if (mutated)
					{
						node = this.Mutate(null, node2);
					}
				}
				else
				{
					if (num < 0)
					{
						ImmutableSortedDictionary<TKey, TValue>.Node node3 = this.left.SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
						if (mutated)
						{
							node = this.Mutate(node3, null);
						}
					}
					else
					{
						if (valueComparer.Equals(this.value, value))
						{
							mutated = false;
							return this;
						}
						if (!overwriteExistingValue)
						{
							throw new ArgumentException(Strings.DuplicateKey);
						}
						mutated = true;
						replacedExistingValue = true;
						node = new ImmutableSortedDictionary<TKey, TValue>.Node(key, value, this.left, this.right, false);
					}
				}
				if (!mutated)
				{
					return node;
				}
				return ImmutableSortedDictionary<TKey, TValue>.Node.MakeBalanced(node);
			}
			private ImmutableSortedDictionary<TKey, TValue>.Node RemoveRecursive(TKey key, IComparer<TKey> keyComparer, out bool mutated)
			{
				if (this.IsEmpty)
				{
					mutated = false;
					return this;
				}
				ImmutableSortedDictionary<TKey, TValue>.Node node = this;
				int num = keyComparer.Compare(key, this.key);
				if (num == 0)
				{
					mutated = true;
					if (this.right.IsEmpty && this.left.IsEmpty)
					{
						node = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
					}
					else
					{
						if (this.right.IsEmpty && !this.left.IsEmpty)
						{
							node = this.left;
						}
						else
						{
							if (!this.right.IsEmpty && this.left.IsEmpty)
							{
								node = this.right;
							}
							else
							{
								ImmutableSortedDictionary<TKey, TValue>.Node node2 = this.right;
								while (!node2.left.IsEmpty)
								{
									node2 = node2.left;
								}
								bool flag;
								ImmutableSortedDictionary<TKey, TValue>.Node node3 = this.right.Remove(node2.key, keyComparer, out flag);
								node = node2.Mutate(this.left, node3);
							}
						}
					}
				}
				else
				{
					if (num < 0)
					{
						ImmutableSortedDictionary<TKey, TValue>.Node node4 = this.left.Remove(key, keyComparer, out mutated);
						if (mutated)
						{
							node = this.Mutate(node4, null);
						}
					}
					else
					{
						ImmutableSortedDictionary<TKey, TValue>.Node node5 = this.right.Remove(key, keyComparer, out mutated);
						if (mutated)
						{
							node = this.Mutate(null, node5);
						}
					}
				}
				if (!node.IsEmpty)
				{
					return ImmutableSortedDictionary<TKey, TValue>.Node.MakeBalanced(node);
				}
				return node;
			}
			private ImmutableSortedDictionary<TKey, TValue>.Node Mutate(ImmutableSortedDictionary<TKey, TValue>.Node left = null, ImmutableSortedDictionary<TKey, TValue>.Node right = null)
			{
				if (this.frozen)
				{
					return new ImmutableSortedDictionary<TKey, TValue>.Node(this.key, this.value, left ?? this.left, right ?? this.right, false);
				}
				if (left != null)
				{
					this.left = left;
				}
				if (right != null)
				{
					this.right = right;
				}
				this.height = 1 + Math.Max(this.left.height, this.right.height);
				return this;
			}
			private ImmutableSortedDictionary<TKey, TValue>.Node Search(TKey key, IComparer<TKey> keyComparer)
			{
				if (this.left == null)
				{
					return this;
				}
				int num = keyComparer.Compare(key, this.key);
				if (num == 0)
				{
					return this;
				}
				if (num > 0)
				{
					return this.right.Search(key, keyComparer);
				}
				return this.left.Search(key, keyComparer);
			}
		}
		[ExcludeFromCodeCoverage]
		private class DebuggerProxy
		{
			private readonly ImmutableSortedDictionary<TKey, TValue> map;
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
			public DebuggerProxy(ImmutableSortedDictionary<TKey, TValue> map)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>>(map, "map");
				this.map = map;
			}
		}
		[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableSortedDictionary<, >.Builder.DebuggerProxy))]
		public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
		{
			[ExcludeFromCodeCoverage]
			private class DebuggerProxy
			{
				private readonly ImmutableSortedDictionary<TKey, TValue>.Builder map;
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
				public DebuggerProxy(ImmutableSortedDictionary<TKey, TValue>.Builder map)
				{
					Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Builder>(map, "map");
					this.map = map;
				}
			}
			private ImmutableSortedDictionary<TKey, TValue>.Node root = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
			private IComparer<TKey> keyComparer = Comparer<TKey>.Default;
			private IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
			private int count;
			private ImmutableSortedDictionary<TKey, TValue> immutable;
			private int version;
			private object syncRoot;
			ICollection<TKey> IDictionary<TKey, TValue>.Keys
			{
				get
				{
					return this.Root.Keys.ToArray(this.Count);
				}
			}
			public IEnumerable<TKey> Keys
			{
				get
				{
					return this.Root.Keys;
				}
			}
			ICollection<TValue> IDictionary<TKey, TValue>.Values
			{
				get
				{
					return this.Root.Values.ToArray(this.Count);
				}
			}
			public IEnumerable<TValue> Values
			{
				get
				{
					return this.Root.Values;
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
			internal int Version
			{
				get
				{
					return this.version;
				}
			}
			private ImmutableSortedDictionary<TKey, TValue>.Node Root
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
					bool flag;
					bool flag2;
					this.Root = this.root.SetItem(key, value, this.keyComparer, this.valueComparer, out flag, out flag2);
					if (flag2 && !flag)
					{
						this.count++;
					}
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
			public IComparer<TKey> KeyComparer
			{
				get
				{
					return this.keyComparer;
				}
				set
				{
					Requires.NotNull<IComparer<TKey>>(value, "value");
					if (value != this.keyComparer)
					{
						ImmutableSortedDictionary<TKey, TValue>.Node node = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
						int num = 0;
						foreach (KeyValuePair<TKey, TValue> current in this)
						{
							bool flag;
							node = node.Add(current.Key, current.Value, value, this.valueComparer, out flag);
							if (flag)
							{
								num++;
							}
						}
						this.keyComparer = value;
						this.Root = node;
						this.count = num;
					}
				}
			}
			public IEqualityComparer<TValue> ValueComparer
			{
				get
				{
					return this.valueComparer;
				}
				set
				{
					Requires.NotNull<IEqualityComparer<TValue>>(value, "value");
					if (value != this.valueComparer)
					{
						this.valueComparer = value;
						this.immutable = null;
					}
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
			internal Builder(ImmutableSortedDictionary<TKey, TValue> map)
			{
				Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>>(map, "map");
				this.root = map.root;
				this.keyComparer = map.KeyComparer;
				this.valueComparer = map.ValueComparer;
				this.count = map.Count;
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
			void ICollection.CopyTo(Array array, int index)
			{
				this.Root.CopyTo(array, index, this.Count);
			}
			public void Add(TKey key, TValue value)
			{
				bool flag;
				this.Root = this.Root.Add(key, value, this.keyComparer, this.valueComparer, out flag);
				if (flag)
				{
					this.count++;
				}
			}
			public bool ContainsKey(TKey key)
			{
				return this.Root.ContainsKey(key, this.keyComparer);
			}
			public bool Remove(TKey key)
			{
				bool flag;
				this.Root = this.Root.Remove(key, this.keyComparer, out flag);
				if (flag)
				{
					this.count--;
				}
				return flag;
			}
			public bool TryGetValue(TKey key, out TValue value)
			{
				return this.Root.TryGetValue(key, this.keyComparer, out value);
			}
			public bool TryGetKey(TKey equalKey, out TKey actualKey)
			{
				Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
				return this.Root.TryGetKey(equalKey, this.keyComparer, out actualKey);
			}
			public void Add(KeyValuePair<TKey, TValue> item)
			{
				this.Add(item.Key, item.Value);
			}
			public void Clear()
			{
				this.Root = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
				this.count = 0;
			}
			public bool Contains(KeyValuePair<TKey, TValue> item)
			{
				return this.Root.Contains(item, this.keyComparer, this.valueComparer);
			}
			void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				this.Root.CopyTo(array, arrayIndex, this.Count);
			}
			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				return this.Contains(item) && this.Remove(item.Key);
			}
			public ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
			{
				return this.Root.GetEnumerator(this);
			}
			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			public bool ContainsValue(TValue value)
			{
				return this.root.ContainsValue(value, this.valueComparer);
			}
			public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
			{
				Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
				foreach (KeyValuePair<TKey, TValue> current in items)
				{
					this.Add(current);
				}
			}
			public void RemoveRange(IEnumerable<TKey> keys)
			{
				Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
				foreach (TKey current in keys)
				{
					this.Remove(current);
				}
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
			public ImmutableSortedDictionary<TKey, TValue> ToImmutable()
			{
				if (this.immutable == null)
				{
					this.immutable = ImmutableSortedDictionary<TKey, TValue>.Wrap(this.Root, this.count, this.keyComparer, this.valueComparer);
				}
				return this.immutable;
			}
		}
		public static readonly ImmutableSortedDictionary<TKey, TValue> Empty = new ImmutableSortedDictionary<TKey, TValue>(null, null);
		private readonly ImmutableSortedDictionary<TKey, TValue>.Node root;
		private readonly int count;
		private readonly IComparer<TKey> keyComparer;
		private readonly IEqualityComparer<TValue> valueComparer;
		public IEqualityComparer<TValue> ValueComparer
		{
			get
			{
				return this.valueComparer;
			}
		}
		public bool IsEmpty
		{
			get
			{
				return this.root.IsEmpty;
			}
		}
		public int Count
		{
			get
			{
				return this.count;
			}
		}
		public IEnumerable<TKey> Keys
		{
			get
			{
				return this.root.Keys;
			}
		}
		public IEnumerable<TValue> Values
		{
			get
			{
				return this.root.Values;
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
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get
			{
				return true;
			}
		}
		public IComparer<TKey> KeyComparer
		{
			get
			{
				return this.keyComparer;
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
		internal ImmutableSortedDictionary(IComparer<TKey> keyComparer = null, IEqualityComparer<TValue> valueComparer = null)
		{
			this.keyComparer = (keyComparer ?? Comparer<TKey>.Default);
			this.valueComparer = (valueComparer ?? EqualityComparer<TValue>.Default);
			this.root = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
		}
		private ImmutableSortedDictionary(ImmutableSortedDictionary<TKey, TValue>.Node root, int count, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(root, "root");
			Requires.Range(count >= 0, "count", null);
			Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
			Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
			root.Freeze(null);
			this.root = root;
			this.count = count;
			this.keyComparer = keyComparer;
			this.valueComparer = valueComparer;
		}
		public ImmutableSortedDictionary<TKey, TValue> Clear()
		{
			if (!this.root.IsEmpty)
			{
				return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(this.keyComparer, this.valueComparer);
			}
			return this;
		}
		IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
		{
			return this.Clear();
		}
		public ImmutableSortedDictionary<TKey, TValue>.Builder ToBuilder()
		{
			return new ImmutableSortedDictionary<TKey, TValue>.Builder(this);
		}
		public ImmutableSortedDictionary<TKey, TValue> Add(TKey key, TValue value)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			bool flag;
			ImmutableSortedDictionary<TKey, TValue>.Node node = this.root.Add(key, value, this.keyComparer, this.valueComparer, out flag);
			return this.Wrap(node, this.count + 1);
		}
		public ImmutableSortedDictionary<TKey, TValue> SetItem(TKey key, TValue value)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			bool flag;
			bool flag2;
			ImmutableSortedDictionary<TKey, TValue>.Node node = this.root.SetItem(key, value, this.keyComparer, this.valueComparer, out flag, out flag2);
			return this.Wrap(node, flag ? this.count : (this.count + 1));
		}
		public ImmutableSortedDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
			return this.AddRange(items, true, false);
		}
		public ImmutableSortedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
			return this.AddRange(items, false, false);
		}
		public ImmutableSortedDictionary<TKey, TValue> Remove(TKey value)
		{
			Requires.NotNullAllowStructs<TKey>(value, "value");
			bool flag;
			ImmutableSortedDictionary<TKey, TValue>.Node node = this.root.Remove(value, this.keyComparer, out flag);
			return this.Wrap(node, this.count - 1);
		}
		public ImmutableSortedDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
		{
			Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
			ImmutableSortedDictionary<TKey, TValue>.Node node = this.root;
			int num = this.count;
			foreach (TKey current in keys)
			{
				bool flag;
				ImmutableSortedDictionary<TKey, TValue>.Node node2 = node.Remove(current, this.keyComparer, out flag);
				if (flag)
				{
					node = node2;
					num--;
				}
			}
			return this.Wrap(node, num);
		}
		public ImmutableSortedDictionary<TKey, TValue> WithComparers(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			if (keyComparer == null)
			{
				keyComparer = Comparer<TKey>.Default;
			}
			if (valueComparer == null)
			{
				valueComparer = EqualityComparer<TValue>.Default;
			}
			if (keyComparer != this.keyComparer)
			{
				ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary = new ImmutableSortedDictionary<TKey, TValue>(ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode, 0, keyComparer, valueComparer);
				return immutableSortedDictionary.AddRange(this, false, true);
			}
			if (valueComparer == this.valueComparer)
			{
				return this;
			}
			return new ImmutableSortedDictionary<TKey, TValue>(this.root, this.count, this.keyComparer, valueComparer);
		}
		public ImmutableSortedDictionary<TKey, TValue> WithComparers(IComparer<TKey> keyComparer)
		{
			return this.WithComparers(keyComparer, this.valueComparer);
		}
		public bool ContainsValue(TValue value)
		{
			return this.root.ContainsValue(value, this.valueComparer);
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
		public bool ContainsKey(TKey key)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return this.root.ContainsKey(key, this.keyComparer);
		}
		public bool Contains(KeyValuePair<TKey, TValue> pair)
		{
			return this.root.Contains(pair, this.keyComparer, this.valueComparer);
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			Requires.NotNullAllowStructs<TKey>(key, "key");
			return this.root.TryGetValue(key, this.keyComparer, out value);
		}
		public bool TryGetKey(TKey equalKey, out TKey actualKey)
		{
			Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
			return this.root.TryGetKey(equalKey, this.keyComparer, out actualKey);
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
		void ICollection.CopyTo(Array array, int index)
		{
			this.root.CopyTo(array, index, this.Count);
		}
		[ExcludeFromCodeCoverage]
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		[ExcludeFromCodeCoverage]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
		{
			return this.root.GetEnumerator();
		}
		private static ImmutableSortedDictionary<TKey, TValue> Wrap(ImmutableSortedDictionary<TKey, TValue>.Node root, int count, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			if (!root.IsEmpty)
			{
				return new ImmutableSortedDictionary<TKey, TValue>(root, count, keyComparer, valueComparer);
			}
			return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
		}
		private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, out ImmutableSortedDictionary<TKey, TValue> other)
		{
			other = (sequence as ImmutableSortedDictionary<TKey, TValue>);
			if (other != null)
			{
				return true;
			}
			ImmutableSortedDictionary<TKey, TValue>.Builder builder = sequence as ImmutableSortedDictionary<TKey, TValue>.Builder;
			if (builder != null)
			{
				other = builder.ToImmutable();
				return true;
			}
			return false;
		}
		private ImmutableSortedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, bool overwriteOnCollision, bool avoidToSortedMap)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
			if (this.IsEmpty && !avoidToSortedMap)
			{
				return this.FillFromEmpty(items, overwriteOnCollision);
			}
			ImmutableSortedDictionary<TKey, TValue>.Node node = this.root;
			int num = this.count;
			foreach (KeyValuePair<TKey, TValue> current in items)
			{
				bool flag = false;
				bool flag2;
				ImmutableSortedDictionary<TKey, TValue>.Node node2 = overwriteOnCollision ? node.SetItem(current.Key, current.Value, this.keyComparer, this.valueComparer, out flag, out flag2) : node.Add(current.Key, current.Value, this.keyComparer, this.valueComparer, out flag2);
				if (flag2)
				{
					node = node2;
					if (!flag)
					{
						num++;
					}
				}
			}
			return this.Wrap(node, num);
		}
		private ImmutableSortedDictionary<TKey, TValue> Wrap(ImmutableSortedDictionary<TKey, TValue>.Node root, int adjustedCountIfDifferentRoot)
		{
			if (this.root == root)
			{
				return this;
			}
			if (!root.IsEmpty)
			{
				return new ImmutableSortedDictionary<TKey, TValue>(root, adjustedCountIfDifferentRoot, this.keyComparer, this.valueComparer);
			}
			return this.Clear();
		}
		private ImmutableSortedDictionary<TKey, TValue> FillFromEmpty(IEnumerable<KeyValuePair<TKey, TValue>> items, bool overwriteOnCollision)
		{
			Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
			ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary;
			if (ImmutableSortedDictionary<TKey, TValue>.TryCastToImmutableMap(items, out immutableSortedDictionary))
			{
				return immutableSortedDictionary.WithComparers(this.KeyComparer, this.ValueComparer);
			}
			IDictionary<TKey, TValue> dictionary = items as IDictionary<TKey, TValue>;
			SortedDictionary<TKey, TValue> sortedDictionary;
			if (dictionary != null)
			{
				sortedDictionary = new SortedDictionary<TKey, TValue>(dictionary, this.KeyComparer);
			}
			else
			{
				sortedDictionary = new SortedDictionary<TKey, TValue>(this.KeyComparer);
				foreach (KeyValuePair<TKey, TValue> current in items)
				{
					if (overwriteOnCollision)
					{
						sortedDictionary[current.Key] = current.Value;
					}
					else
					{
						TValue x;
						if (sortedDictionary.TryGetValue(current.Key, out x))
						{
							if (!this.valueComparer.Equals(x, current.Value))
							{
								throw new ArgumentException(Strings.DuplicateKey);
							}
						}
						else
						{
							sortedDictionary.Add(current.Key, current.Value);
						}
					}
				}
			}
			if (sortedDictionary.Count == 0)
			{
				return this;
			}
			ImmutableSortedDictionary<TKey, TValue>.Node node = ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromSortedDictionary(sortedDictionary);
			return new ImmutableSortedDictionary<TKey, TValue>(node, sortedDictionary.Count, this.KeyComparer, this.ValueComparer);
		}
	}
}
