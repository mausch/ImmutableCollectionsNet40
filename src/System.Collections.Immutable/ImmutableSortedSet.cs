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
	public static class ImmutableSortedSet
	{
		public static ImmutableSortedSet<T> Create<T>()
		{
			return ImmutableSortedSet<T>.Empty;
		}
		public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer)
		{
			return ImmutableSortedSet<T>.Empty.WithComparer(comparer);
		}
		public static ImmutableSortedSet<T> Create<T>(T item)
		{
			return ImmutableSortedSet<T>.Empty.Add(item);
		}
		public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer, T item)
		{
			return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Add(item);
		}
		public static ImmutableSortedSet<T> CreateRange<T>(IEnumerable<T> items)
		{
			return ImmutableSortedSet<T>.Empty.Union(items);
		}
		public static ImmutableSortedSet<T> CreateRange<T>(IComparer<T> comparer, IEnumerable<T> items)
		{
			return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Union(items);
		}
		public static ImmutableSortedSet<T> Create<T>(params T[] items)
		{
			return ImmutableSortedSet<T>.Empty.Union(items);
		}
		public static ImmutableSortedSet<T> Create<T>(IComparer<T> comparer, params T[] items)
		{
			return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Union(items);
		}
		public static ImmutableSortedSet<T>.Builder CreateBuilder<T>()
		{
			return ImmutableSortedSet.Create<T>().ToBuilder();
		}
		public static ImmutableSortedSet<T>.Builder CreateBuilder<T>(IComparer<T> comparer)
		{
			return ImmutableSortedSet.Create<T>(comparer).ToBuilder();
		}
		public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
		{
			ImmutableSortedSet<TSource> immutableSortedSet = source as ImmutableSortedSet<TSource>;
			if (immutableSortedSet != null)
			{
				return immutableSortedSet.WithComparer(comparer);
			}
			return ImmutableSortedSet<TSource>.Empty.WithComparer(comparer).Union(source);
		}
		public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(this IEnumerable<TSource> source)
		{
			return source.ToImmutableSortedSet(null);
		}
	}
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableSortedSet<>.DebuggerProxy))]
	public sealed class ImmutableSortedSet<T> : IImmutableSet<T>, ISortKeyCollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IList<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
	{
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable, ISecurePooledObjectUser
		{
			private static readonly SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableSortedSet<T>.Enumerator> enumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableSortedSet<T>.Enumerator>();
			private readonly ImmutableSortedSet<T>.Builder builder;
			private readonly Guid poolUserId;
			private readonly bool reverse;
			private IBinaryTree<T> root;
			private SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>> stack;
			private IBinaryTree<T> current;
			private int enumeratingBuilderVersion;
			Guid ISecurePooledObjectUser.PoolUserId
			{
				get
				{
					return this.poolUserId;
				}
			}
			public T Current
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
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			internal Enumerator(IBinaryTree<T> root, ImmutableSortedSet<T>.Builder builder = null, bool reverse = false)
			{
				Requires.NotNull<IBinaryTree<T>>(root, "root");
				this.root = root;
				this.builder = builder;
				this.current = null;
				this.reverse = reverse;
				this.enumeratingBuilderVersion = ((builder != null) ? builder.Version : -1);
				this.poolUserId = Guid.NewGuid();
				this.stack = null;
				if (!ImmutableSortedSet<T>.Enumerator.enumeratingStacks.TryTake(this, out this.stack))
				{
					this.stack = ImmutableSortedSet<T>.Enumerator.enumeratingStacks.PrepNew(this, new Stack<RefAsValueType<IBinaryTree<T>>>(root.Height));
				}
				this.Reset();
			}
			public void Dispose()
			{
				this.root = null;
				this.current = null;
				if (this.stack != null && this.stack.Owner == this.poolUserId)
				{
					using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this))
					{
						securePooledObjectUser.Value.Clear();
					}
					ImmutableSortedSet<T>.Enumerator.enumeratingStacks.TryAdd(this, this.stack);
					this.stack = null;
				}
			}
			public bool MoveNext()
			{
				this.ThrowIfDisposed();
				this.ThrowIfChanged();
				bool result;
				using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this))
				{
					if (securePooledObjectUser.Value.Count > 0)
					{
						IBinaryTree<T> value = securePooledObjectUser.Value.Pop().Value;
						this.current = value;
						this.PushNext(this.reverse ? value.Left : value.Right);
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
				using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this))
				{
					securePooledObjectUser.Value.Clear();
				}
				this.PushNext(this.root);
			}
			private void ThrowIfDisposed()
			{
				if (this.root == null)
				{
					throw new ObjectDisposedException(base.GetType().FullName);
				}
				if (this.stack != null)
				{
					this.stack.ThrowDisposedIfNotOwned<ImmutableSortedSet<T>.Enumerator>(this);
				}
			}
			private void ThrowIfChanged()
			{
				if (this.builder != null && this.builder.Version != this.enumeratingBuilderVersion)
				{
					throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
				}
			}
			private void PushNext(IBinaryTree<T> node)
			{
				Requires.NotNull<IBinaryTree<T>>(node, "node");
				using (SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this))
				{
					while (!node.IsEmpty)
					{
						securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(node));
						node = (this.reverse ? node.Right : node.Left);
					}
				}
			}
		}
		private class ReverseEnumerable : IEnumerable<T>, IEnumerable
		{
			private readonly ImmutableSortedSet<T>.Node root;
			internal ReverseEnumerable(ImmutableSortedSet<T>.Node root)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(root, "root");
				this.root = root;
			}
			public IEnumerator<T> GetEnumerator()
			{
				return this.root.Reverse();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}
		[DebuggerDisplay("{key}")]
		private sealed class Node : IBinaryTree<T>, IEnumerable<T>, IEnumerable
		{
			internal static readonly ImmutableSortedSet<T>.Node EmptyNode = new ImmutableSortedSet<T>.Node();
			private readonly T key;
			private bool frozen;
			private int height;
			private int count;
			private ImmutableSortedSet<T>.Node left;
			private ImmutableSortedSet<T>.Node right;
			public bool IsEmpty
			{
				get
				{
					return this.left == null;
				}
			}
			int IBinaryTree<T>.Height
			{
				get
				{
					return this.height;
				}
			}
			IBinaryTree<T> IBinaryTree<T>.Left
			{
				get
				{
					return this.left;
				}
			}
			IBinaryTree<T> IBinaryTree<T>.Right
			{
				get
				{
					return this.right;
				}
			}
			T IBinaryTree<T>.Value
			{
				get
				{
					return this.key;
				}
			}
			public int Count
			{
				get
				{
					return this.count;
				}
			}
			internal T Key
			{
				get
				{
					return this.key;
				}
			}
			internal T Max
			{
				get
				{
					if (this.IsEmpty)
					{
						return default(T);
					}
					ImmutableSortedSet<T>.Node node = this;
					while (!node.right.IsEmpty)
					{
						node = node.right;
					}
					return node.key;
				}
			}
			internal T Min
			{
				get
				{
					if (this.IsEmpty)
					{
						return default(T);
					}
					ImmutableSortedSet<T>.Node node = this;
					while (!node.left.IsEmpty)
					{
						node = node.left;
					}
					return node.key;
				}
			}
			internal T this[int index]
			{
				get
				{
					Requires.Range(index >= 0 && index < this.Count, "index", null);
					if (index < this.left.count)
					{
						return this.left[index];
					}
					if (index > this.left.count)
					{
						return this.right[index - this.left.count - 1];
					}
					return this.key;
				}
			}
			private Node()
			{
				this.frozen = true;
			}
			private Node(T key, ImmutableSortedSet<T>.Node left, ImmutableSortedSet<T>.Node right, bool frozen = false)
			{
				Requires.NotNullAllowStructs<T>(key, "key");
				Requires.NotNull<ImmutableSortedSet<T>.Node>(left, "left");
				Requires.NotNull<ImmutableSortedSet<T>.Node>(right, "right");
				this.key = key;
				this.left = left;
				this.right = right;
				this.height = 1 + Math.Max(left.height, right.height);
				this.count = 1 + left.count + right.count;
				this.frozen = frozen;
			}
			public ImmutableSortedSet<T>.Enumerator GetEnumerator()
			{
				return new ImmutableSortedSet<T>.Enumerator(this, null, false);
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
			internal ImmutableSortedSet<T>.Enumerator GetEnumerator(ImmutableSortedSet<T>.Builder builder)
			{
				return new ImmutableSortedSet<T>.Enumerator(this, builder, false);
			}
			internal static ImmutableSortedSet<T>.Node NodeTreeFromSortedSet(SortedSet<T> collection)
			{
				Requires.NotNull<SortedSet<T>>(collection, "collection");
				if (collection.Count == 0)
				{
					return ImmutableSortedSet<T>.Node.EmptyNode;
				}
				IOrderedCollection<T> orderedCollection = collection.AsOrderedCollection<T>();
				return ImmutableSortedSet<T>.Node.NodeTreeFromList(orderedCollection, 0, orderedCollection.Count);
			}
			internal void CopyTo(T[] array, int arrayIndex)
			{
				Requires.NotNull<T[]>(array, "array");
				Requires.Range(arrayIndex >= 0, "arrayIndex", null);
				Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
				foreach (T current in this)
				{
					array[arrayIndex++] = current;
				}
			}
			internal void CopyTo(Array array, int arrayIndex)
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
			internal ImmutableSortedSet<T>.Node Add(T key, IComparer<T> comparer, out bool mutated)
			{
				Requires.NotNullAllowStructs<T>(key, "key");
				Requires.NotNull<IComparer<T>>(comparer, "comparer");
				if (this.IsEmpty)
				{
					mutated = true;
					return new ImmutableSortedSet<T>.Node(key, this, this, false);
				}
				ImmutableSortedSet<T>.Node node = this;
				int num = comparer.Compare(key, this.key);
				if (num > 0)
				{
					ImmutableSortedSet<T>.Node node2 = this.right.Add(key, comparer, out mutated);
					if (mutated)
					{
						node = this.Mutate(null, node2);
					}
				}
				else
				{
					if (num >= 0)
					{
						mutated = false;
						return this;
					}
					ImmutableSortedSet<T>.Node node3 = this.left.Add(key, comparer, out mutated);
					if (mutated)
					{
						node = this.Mutate(node3, null);
					}
				}
				if (!mutated)
				{
					return node;
				}
				return ImmutableSortedSet<T>.Node.MakeBalanced(node);
			}
			internal ImmutableSortedSet<T>.Node Remove(T key, IComparer<T> comparer, out bool mutated)
			{
				Requires.NotNullAllowStructs<T>(key, "key");
				Requires.NotNull<IComparer<T>>(comparer, "comparer");
				if (this.IsEmpty)
				{
					mutated = false;
					return this;
				}
				ImmutableSortedSet<T>.Node node = this;
				int num = comparer.Compare(key, this.key);
				if (num == 0)
				{
					mutated = true;
					if (this.right.IsEmpty && this.left.IsEmpty)
					{
						node = ImmutableSortedSet<T>.Node.EmptyNode;
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
								ImmutableSortedSet<T>.Node node2 = this.right;
								while (!node2.left.IsEmpty)
								{
									node2 = node2.left;
								}
								bool flag;
								ImmutableSortedSet<T>.Node node3 = this.right.Remove(node2.key, comparer, out flag);
								node = node2.Mutate(this.left, node3);
							}
						}
					}
				}
				else
				{
					if (num < 0)
					{
						ImmutableSortedSet<T>.Node node4 = this.left.Remove(key, comparer, out mutated);
						if (mutated)
						{
							node = this.Mutate(node4, null);
						}
					}
					else
					{
						ImmutableSortedSet<T>.Node node5 = this.right.Remove(key, comparer, out mutated);
						if (mutated)
						{
							node = this.Mutate(null, node5);
						}
					}
				}
				if (!node.IsEmpty)
				{
					return ImmutableSortedSet<T>.Node.MakeBalanced(node);
				}
				return node;
			}
			internal bool Contains(T key, IComparer<T> comparer)
			{
				Requires.NotNullAllowStructs<T>(key, "key");
				Requires.NotNull<IComparer<T>>(comparer, "comparer");
				return !this.Search(key, comparer).IsEmpty;
			}
			internal void Freeze()
			{
				if (!this.frozen)
				{
					this.left.Freeze();
					this.right.Freeze();
					this.frozen = true;
				}
			}
			internal ImmutableSortedSet<T>.Node Search(T key, IComparer<T> comparer)
			{
				Requires.NotNullAllowStructs<T>(key, "key");
				Requires.NotNull<IComparer<T>>(comparer, "comparer");
				if (this.IsEmpty)
				{
					return this;
				}
				int num = comparer.Compare(key, this.key);
				if (num == 0)
				{
					return this;
				}
				if (num > 0)
				{
					return this.right.Search(key, comparer);
				}
				return this.left.Search(key, comparer);
			}
			internal int IndexOf(T key, IComparer<T> comparer)
			{
				Requires.NotNullAllowStructs<T>(key, "key");
				Requires.NotNull<IComparer<T>>(comparer, "comparer");
				if (this.IsEmpty)
				{
					return -1;
				}
				int num = comparer.Compare(key, this.key);
				if (num == 0)
				{
					return this.left.Count;
				}
				if (num > 0)
				{
					int num2 = this.right.IndexOf(key, comparer);
					bool flag = num2 < 0;
					if (flag)
					{
						num2 = ~num2;
					}
					num2 = this.left.Count + 1 + num2;
					if (flag)
					{
						num2 = ~num2;
					}
					return num2;
				}
				return this.left.IndexOf(key, comparer);
			}
			internal IEnumerator<T> Reverse()
			{
				return new ImmutableSortedSet<T>.Enumerator(this, null, true);
			}
			private static ImmutableSortedSet<T>.Node RotateLeft(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				if (tree.right.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedSet<T>.Node node = tree.right;
				return node.Mutate(tree.Mutate(null, node.left), null);
			}
			private static ImmutableSortedSet<T>.Node RotateRight(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				if (tree.left.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedSet<T>.Node node = tree.left;
				return node.Mutate(null, tree.Mutate(node.right, null));
			}
			private static ImmutableSortedSet<T>.Node DoubleLeft(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				if (tree.right.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedSet<T>.Node tree2 = tree.Mutate(null, ImmutableSortedSet<T>.Node.RotateRight(tree.right));
				return ImmutableSortedSet<T>.Node.RotateLeft(tree2);
			}
			private static ImmutableSortedSet<T>.Node DoubleRight(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				if (tree.left.IsEmpty)
				{
					return tree;
				}
				ImmutableSortedSet<T>.Node tree2 = tree.Mutate(ImmutableSortedSet<T>.Node.RotateLeft(tree.left), null);
				return ImmutableSortedSet<T>.Node.RotateRight(tree2);
			}
			private static int Balance(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				return tree.right.height - tree.left.height;
			}
			private static bool IsRightHeavy(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				return ImmutableSortedSet<T>.Node.Balance(tree) >= 2;
			}
			private static bool IsLeftHeavy(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				return ImmutableSortedSet<T>.Node.Balance(tree) <= -2;
			}
			private static ImmutableSortedSet<T>.Node MakeBalanced(ImmutableSortedSet<T>.Node tree)
			{
				Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
				if (ImmutableSortedSet<T>.Node.IsRightHeavy(tree))
				{
					if (!ImmutableSortedSet<T>.Node.IsLeftHeavy(tree.right))
					{
						return ImmutableSortedSet<T>.Node.RotateLeft(tree);
					}
					return ImmutableSortedSet<T>.Node.DoubleLeft(tree);
				}
				else
				{
					if (!ImmutableSortedSet<T>.Node.IsLeftHeavy(tree))
					{
						return tree;
					}
					if (!ImmutableSortedSet<T>.Node.IsRightHeavy(tree.left))
					{
						return ImmutableSortedSet<T>.Node.RotateRight(tree);
					}
					return ImmutableSortedSet<T>.Node.DoubleRight(tree);
				}
			}
			private static ImmutableSortedSet<T>.Node NodeTreeFromList(IOrderedCollection<T> items, int start, int length)
			{
				Requires.NotNull<IOrderedCollection<T>>(items, "items");
				if (length == 0)
				{
					return ImmutableSortedSet<T>.Node.EmptyNode;
				}
				int num = (length - 1) / 2;
				int num2 = length - 1 - num;
				ImmutableSortedSet<T>.Node node = ImmutableSortedSet<T>.Node.NodeTreeFromList(items, start, num2);
				ImmutableSortedSet<T>.Node node2 = ImmutableSortedSet<T>.Node.NodeTreeFromList(items, start + num2 + 1, num);
				return new ImmutableSortedSet<T>.Node(items[start + num2], node, node2, true);
			}
			private ImmutableSortedSet<T>.Node Mutate(ImmutableSortedSet<T>.Node left = null, ImmutableSortedSet<T>.Node right = null)
			{
				if (this.frozen)
				{
					return new ImmutableSortedSet<T>.Node(this.key, left ?? this.left, right ?? this.right, false);
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
				this.count = 1 + this.left.count + this.right.count;
				return this;
			}
		}
		[ExcludeFromCodeCoverage]
		private class DebuggerProxy
		{
			private readonly ImmutableSortedSet<T> set;
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
			public DebuggerProxy(ImmutableSortedSet<T> set)
			{
				Requires.NotNull<ImmutableSortedSet<T>>(set, "set");
				this.set = set;
			}
		}
		[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableSortedSet<>.Builder.DebuggerProxy))]
		public sealed class Builder : ISortKeyCollection<T>, IReadOnlyCollection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
		{
			[ExcludeFromCodeCoverage]
			private class DebuggerProxy
			{
				private readonly ImmutableSortedSet<T>.Node set;
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
				public DebuggerProxy(ImmutableSortedSet<T>.Builder builder)
				{
					Requires.NotNull<ImmutableSortedSet<T>.Builder>(builder, "builder");
					this.set = builder.Root;
				}
			}
			private ImmutableSortedSet<T>.Node root = ImmutableSortedSet<T>.Node.EmptyNode;
			private IComparer<T> comparer = Comparer<T>.Default;
			private ImmutableSortedSet<T> immutable;
			private int version;
			private object syncRoot;
			public int Count
			{
				get
				{
					return this.Root.Count;
				}
			}
			bool ICollection<T>.IsReadOnly
			{
				get
				{
					return false;
				}
			}
			public T Max
			{
				get
				{
					return this.root.Max;
				}
			}
			public T Min
			{
				get
				{
					return this.root.Min;
				}
			}
			public IComparer<T> KeyComparer
			{
				get
				{
					return this.comparer;
				}
				set
				{
					Requires.NotNull<IComparer<T>>(value, "value");
					if (value != this.comparer)
					{
						ImmutableSortedSet<T>.Node node = ImmutableSortedSet<T>.Node.EmptyNode;
						foreach (T current in this)
						{
							bool flag;
							node = node.Add(current, value, out flag);
						}
						this.immutable = null;
						this.comparer = value;
						this.Root = node;
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
			private ImmutableSortedSet<T>.Node Root
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
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			bool ICollection.IsSynchronized
			{
				get
				{
					return false;
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
			internal Builder(ImmutableSortedSet<T> set)
			{
				Requires.NotNull<ImmutableSortedSet<T>>(set, "set");
				this.root = set.root;
				this.comparer = set.KeyComparer;
				this.immutable = set;
			}
			public bool Add(T item)
			{
				bool result;
				this.Root = this.Root.Add(item, this.comparer, out result);
				return result;
			}
			public void ExceptWith(IEnumerable<T> other)
			{
				Requires.NotNull<IEnumerable<T>>(other, "other");
				foreach (T current in other)
				{
					bool flag;
					this.Root = this.Root.Remove(current, this.comparer, out flag);
				}
			}
			public void IntersectWith(IEnumerable<T> other)
			{
				Requires.NotNull<IEnumerable<T>>(other, "other");
				ImmutableSortedSet<T>.Node node = ImmutableSortedSet<T>.Node.EmptyNode;
				foreach (T current in other)
				{
					if (this.Contains(current))
					{
						bool flag;
						node = node.Add(current, this.comparer, out flag);
					}
				}
				this.Root = node;
			}
			public bool IsProperSubsetOf(IEnumerable<T> other)
			{
				return this.ToImmutable().IsProperSubsetOf(other);
			}
			public bool IsProperSupersetOf(IEnumerable<T> other)
			{
				return this.ToImmutable().IsProperSupersetOf(other);
			}
			public bool IsSubsetOf(IEnumerable<T> other)
			{
				return this.ToImmutable().IsSubsetOf(other);
			}
			public bool IsSupersetOf(IEnumerable<T> other)
			{
				return this.ToImmutable().IsSupersetOf(other);
			}
			public bool Overlaps(IEnumerable<T> other)
			{
				return this.ToImmutable().Overlaps(other);
			}
			public bool SetEquals(IEnumerable<T> other)
			{
				return this.ToImmutable().SetEquals(other);
			}
			public void SymmetricExceptWith(IEnumerable<T> other)
			{
				this.Root = this.ToImmutable().SymmetricExcept(other).root;
			}
			public void UnionWith(IEnumerable<T> other)
			{
				Requires.NotNull<IEnumerable<T>>(other, "other");
				foreach (T current in other)
				{
					bool flag;
					this.Root = this.Root.Add(current, this.comparer, out flag);
				}
			}
			void ICollection<T>.Add(T item)
			{
				this.Add(item);
			}
			public void Clear()
			{
				this.Root = ImmutableSortedSet<T>.Node.EmptyNode;
			}
			public bool Contains(T item)
			{
				return this.Root.Contains(item, this.comparer);
			}
			void ICollection<T>.CopyTo(T[] array, int arrayIndex)
			{
				this.root.CopyTo(array, arrayIndex);
			}
			public bool Remove(T item)
			{
				bool result;
				this.Root = this.Root.Remove(item, this.comparer, out result);
				return result;
			}
			public ImmutableSortedSet<T>.Enumerator GetEnumerator()
			{
				return this.Root.GetEnumerator(this);
			}
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return this.Root.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
			public IEnumerable<T> Reverse()
			{
				return new ImmutableSortedSet<T>.ReverseEnumerable(this.root);
			}
			public ImmutableSortedSet<T> ToImmutable()
			{
				if (this.immutable == null)
				{
					this.immutable = ImmutableSortedSet<T>.Wrap(this.Root, this.comparer);
				}
				return this.immutable;
			}
			void ICollection.CopyTo(Array array, int arrayIndex)
			{
				this.Root.CopyTo(array, arrayIndex);
			}
		}
		private const float RefillOverIncrementalThreshold = 0.15f;
		public static readonly ImmutableSortedSet<T> Empty = new ImmutableSortedSet<T>(null);
		private readonly ImmutableSortedSet<T>.Node root;
		private readonly IComparer<T> comparer;
		public T Max
		{
			get
			{
				return this.root.Max;
			}
		}
		public T Min
		{
			get
			{
				return this.root.Min;
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
				return this.root.Count;
			}
		}
		public IComparer<T> KeyComparer
		{
			get
			{
				return this.comparer;
			}
		}
		public T this[int index]
		{
			get
			{
				return this.root[index];
			}
		}
		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return true;
			}
		}
		T IList<T>.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		bool IList.IsFixedSize
		{
			get
			{
				return true;
			}
		}
		bool IList.IsReadOnly
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
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized
		{
			get
			{
				return true;
			}
		}
		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		internal ImmutableSortedSet(IComparer<T> comparer = null)
		{
			this.root = ImmutableSortedSet<T>.Node.EmptyNode;
			this.comparer = (comparer ?? Comparer<T>.Default);
		}
		private ImmutableSortedSet(ImmutableSortedSet<T>.Node root, IComparer<T> comparer)
		{
			Requires.NotNull<ImmutableSortedSet<T>.Node>(root, "root");
			Requires.NotNull<IComparer<T>>(comparer, "comparer");
			root.Freeze();
			this.root = root;
			this.comparer = comparer;
		}
		public ImmutableSortedSet<T> Clear()
		{
			if (!this.root.IsEmpty)
			{
				return ImmutableSortedSet<T>.Empty.WithComparer(this.comparer);
			}
			return this;
		}
		public ImmutableSortedSet<T>.Builder ToBuilder()
		{
			return new ImmutableSortedSet<T>.Builder(this);
		}
		public ImmutableSortedSet<T> Add(T value)
		{
			Requires.NotNullAllowStructs<T>(value, "value");
			bool flag;
			return this.Wrap(this.root.Add(value, this.comparer, out flag));
		}
		public ImmutableSortedSet<T> Remove(T value)
		{
			Requires.NotNullAllowStructs<T>(value, "value");
			bool flag;
			return this.Wrap(this.root.Remove(value, this.comparer, out flag));
		}
		public bool TryGetValue(T equalValue, out T actualValue)
		{
			Requires.NotNullAllowStructs<T>(equalValue, "equalValue");
			ImmutableSortedSet<T>.Node node = this.root.Search(equalValue, this.comparer);
			if (node.IsEmpty)
			{
				actualValue = equalValue;
				return false;
			}
			actualValue = node.Key;
			return true;
		}
		public ImmutableSortedSet<T> Intersect(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			ImmutableSortedSet<T> immutableSortedSet = this.Clear();
			foreach (T current in other)
			{
				if (this.Contains(current))
				{
					immutableSortedSet = immutableSortedSet.Add(current);
				}
			}
			return immutableSortedSet;
		}
		public ImmutableSortedSet<T> Except(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			ImmutableSortedSet<T>.Node node = this.root;
			foreach (T current in other)
			{
				bool flag;
				node = node.Remove(current, this.comparer, out flag);
			}
			return this.Wrap(node);
		}
		public ImmutableSortedSet<T> SymmetricExcept(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			ImmutableSortedSet<T> immutableSortedSet = ImmutableSortedSet<T>.Empty.Union(other);
			ImmutableSortedSet<T> immutableSortedSet2 = this.Clear();
			foreach (T current in this)
			{
				if (!immutableSortedSet.Contains(current))
				{
					immutableSortedSet2 = immutableSortedSet2.Add(current);
				}
			}
			foreach (T current2 in immutableSortedSet)
			{
				if (!this.Contains(current2))
				{
					immutableSortedSet2 = immutableSortedSet2.Add(current2);
				}
			}
			return immutableSortedSet2;
		}
		public ImmutableSortedSet<T> Union(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			ImmutableSortedSet<T> immutableSortedSet;
			if (ImmutableSortedSet<T>.TryCastToImmutableSortedSet(other, out immutableSortedSet) && immutableSortedSet.KeyComparer == this.KeyComparer)
			{
				if (immutableSortedSet.IsEmpty)
				{
					return this;
				}
				if (this.IsEmpty)
				{
					return immutableSortedSet;
				}
				if (immutableSortedSet.Count > this.Count)
				{
					return immutableSortedSet.Union(this);
				}
			}
			int num;
			if (this.IsEmpty || (other.TryGetCount(out num) && (float)(this.Count + num) * 0.15f > (float)this.Count))
			{
				return this.LeafToRootRefill(other);
			}
			return this.UnionIncremental(other);
		}
		public ImmutableSortedSet<T> WithComparer(IComparer<T> comparer)
		{
			if (comparer == null)
			{
				comparer = Comparer<T>.Default;
			}
			if (comparer == this.comparer)
			{
				return this;
			}
			ImmutableSortedSet<T> immutableSortedSet = new ImmutableSortedSet<T>(ImmutableSortedSet<T>.Node.EmptyNode, comparer);
			return immutableSortedSet.Union(this);
		}
		public bool SetEquals(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			SortedSet<T> sortedSet = new SortedSet<T>(other, this.KeyComparer);
			if (this.Count != sortedSet.Count)
			{
				return false;
			}
			int num = 0;
			foreach (T current in sortedSet)
			{
				if (!this.Contains(current))
				{
					return false;
				}
				num++;
			}
			return num == this.Count;
		}
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (this.IsEmpty)
			{
				return other.Any<T>();
			}
			SortedSet<T> sortedSet = new SortedSet<T>(other, this.KeyComparer);
			if (this.Count >= sortedSet.Count)
			{
				return false;
			}
			int num = 0;
			bool flag = false;
			foreach (T current in sortedSet)
			{
				if (this.Contains(current))
				{
					num++;
				}
				else
				{
					flag = true;
				}
				if (num == this.Count && flag)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (this.IsEmpty)
			{
				return false;
			}
			int num = 0;
			foreach (T current in other)
			{
				num++;
				if (!this.Contains(current))
				{
					return false;
				}
			}
			return this.Count > num;
		}
		public bool IsSubsetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (this.IsEmpty)
			{
				return true;
			}
			SortedSet<T> sortedSet = new SortedSet<T>(other, this.KeyComparer);
			int num = 0;
			foreach (T current in sortedSet)
			{
				if (this.Contains(current))
				{
					num++;
				}
			}
			return num == this.Count;
		}
		public bool IsSupersetOf(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			foreach (T current in other)
			{
				if (!this.Contains(current))
				{
					return false;
				}
			}
			return true;
		}
		public bool Overlaps(IEnumerable<T> other)
		{
			Requires.NotNull<IEnumerable<T>>(other, "other");
			if (this.IsEmpty)
			{
				return false;
			}
			foreach (T current in other)
			{
				if (this.Contains(current))
				{
					return true;
				}
			}
			return false;
		}
		public IEnumerable<T> Reverse()
		{
			return new ImmutableSortedSet<T>.ReverseEnumerable(this.root);
		}
		public int IndexOf(T item)
		{
			Requires.NotNullAllowStructs<T>(item, "item");
			return this.root.IndexOf(item, this.comparer);
		}
		public bool Contains(T value)
		{
			Requires.NotNullAllowStructs<T>(value, "value");
			return this.root.Contains(value, this.comparer);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Clear()
		{
			return this.Clear();
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Add(T value)
		{
			return this.Add(value);
		}
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Remove(T value)
		{
			return this.Remove(value);
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
		[ExcludeFromCodeCoverage]
		IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
		{
			return this.Union(other);
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
			this.root.CopyTo(array, arrayIndex);
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
		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException();
		}
		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		int IList.Add(object value)
		{
			throw new NotSupportedException();
		}
		void IList.Clear()
		{
			throw new NotSupportedException();
		}
		bool IList.Contains(object value)
		{
			return this.Contains((T)((object)value));
		}
		int IList.IndexOf(object value)
		{
			return this.IndexOf((T)((object)value));
		}
		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}
		void IList.Remove(object value)
		{
			throw new NotSupportedException();
		}
		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		void ICollection.CopyTo(Array array, int index)
		{
			this.root.CopyTo(array, index);
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
		public ImmutableSortedSet<T>.Enumerator GetEnumerator()
		{
			return this.root.GetEnumerator();
		}
		private static bool TryCastToImmutableSortedSet(IEnumerable<T> sequence, out ImmutableSortedSet<T> other)
		{
			other = (sequence as ImmutableSortedSet<T>);
			if (other != null)
			{
				return true;
			}
			ImmutableSortedSet<T>.Builder builder = sequence as ImmutableSortedSet<T>.Builder;
			if (builder != null)
			{
				other = builder.ToImmutable();
				return true;
			}
			return false;
		}
		private static ImmutableSortedSet<T> Wrap(ImmutableSortedSet<T>.Node root, IComparer<T> comparer)
		{
			if (!root.IsEmpty)
			{
				return new ImmutableSortedSet<T>(root, comparer);
			}
			return ImmutableSortedSet<T>.Empty.WithComparer(comparer);
		}
		private ImmutableSortedSet<T> UnionIncremental(IEnumerable<T> items)
		{
			Requires.NotNull<IEnumerable<T>>(items, "items");
			ImmutableSortedSet<T>.Node node = this.root;
			foreach (T current in items)
			{
				bool flag;
				node = node.Add(current, this.comparer, out flag);
			}
			return this.Wrap(node);
		}
		private ImmutableSortedSet<T> Wrap(ImmutableSortedSet<T>.Node root)
		{
			if (root == this.root)
			{
				return this;
			}
			if (!root.IsEmpty)
			{
				return new ImmutableSortedSet<T>(root, this.comparer);
			}
			return this.Clear();
		}
		private ImmutableSortedSet<T> LeafToRootRefill(IEnumerable<T> addedItems)
		{
			Requires.NotNull<IEnumerable<T>>(addedItems, "addedItems");
			SortedSet<T> collection = new SortedSet<T>(this.Concat(addedItems), this.KeyComparer);
			ImmutableSortedSet<T>.Node node = ImmutableSortedSet<T>.Node.NodeTreeFromSortedSet(collection);
			return this.Wrap(node);
		}
	}
}
