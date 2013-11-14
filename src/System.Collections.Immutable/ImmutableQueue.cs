using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Validation;
namespace System.Collections.Immutable
{
	public static class ImmutableQueue
	{
		public static ImmutableQueue<T> Create<T>()
		{
			return ImmutableQueue<T>.Empty;
		}
		public static ImmutableQueue<T> Create<T>(T item)
		{
			return ImmutableQueue<T>.Empty.Enqueue(item);
		}
		public static ImmutableQueue<T> CreateRange<T>(IEnumerable<T> items)
		{
			Requires.NotNull<IEnumerable<T>>(items, "items");
			ImmutableQueue<T> immutableQueue = ImmutableQueue<T>.Empty;
			foreach (T current in items)
			{
				immutableQueue = immutableQueue.Enqueue(current);
			}
			return immutableQueue;
		}
		public static ImmutableQueue<T> Create<T>(params T[] items)
		{
			Requires.NotNull<T[]>(items, "items");
			ImmutableQueue<T> immutableQueue = ImmutableQueue<T>.Empty;
			for (int i = 0; i < items.Length; i++)
			{
				T value = items[i];
				immutableQueue = immutableQueue.Enqueue(value);
			}
			return immutableQueue;
		}
		public static IImmutableQueue<T> Dequeue<T>(this IImmutableQueue<T> queue, out T value)
		{
			Requires.NotNull<IImmutableQueue<T>>(queue, "queue");
			value = queue.Peek();
			return queue.Dequeue();
		}
	}
	[DebuggerDisplay("IsEmpty = {IsEmpty}"), DebuggerTypeProxy(typeof(ImmutableQueue<>.DebuggerProxy))]
	public sealed class ImmutableQueue<T> : IImmutableQueue<T>, IEnumerable<T>, IEnumerable
	{
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public struct Enumerator
		{
			private readonly ImmutableQueue<T> originalQueue;
			private ImmutableStack<T> remainingForwardsStack;
			private ImmutableStack<T> remainingBackwardsStack;
			public T Current
			{
				get
				{
					if (this.remainingForwardsStack == null)
					{
						throw new InvalidOperationException();
					}
					if (!this.remainingForwardsStack.IsEmpty)
					{
						return this.remainingForwardsStack.Peek();
					}
					if (!this.remainingBackwardsStack.IsEmpty)
					{
						return this.remainingBackwardsStack.Peek();
					}
					throw new InvalidOperationException();
				}
			}
			internal Enumerator(ImmutableQueue<T> queue)
			{
				this.originalQueue = queue;
				this.remainingForwardsStack = null;
				this.remainingBackwardsStack = null;
			}
			public bool MoveNext()
			{
				if (this.remainingForwardsStack == null)
				{
					this.remainingForwardsStack = this.originalQueue.forwards;
					this.remainingBackwardsStack = this.originalQueue.BackwardsReversed;
				}
				else
				{
					if (!this.remainingForwardsStack.IsEmpty)
					{
						this.remainingForwardsStack = this.remainingForwardsStack.Pop();
					}
					else
					{
						if (!this.remainingBackwardsStack.IsEmpty)
						{
							this.remainingBackwardsStack = this.remainingBackwardsStack.Pop();
						}
					}
				}
				return !this.remainingForwardsStack.IsEmpty || !this.remainingBackwardsStack.IsEmpty;
			}
		}
		private class EnumeratorObject : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly ImmutableQueue<T> originalQueue;
			private ImmutableStack<T> remainingForwardsStack;
			private ImmutableStack<T> remainingBackwardsStack;
			private bool disposed;
			public T Current
			{
				get
				{
					this.ThrowIfDisposed();
					if (this.remainingForwardsStack == null)
					{
						throw new InvalidOperationException();
					}
					if (!this.remainingForwardsStack.IsEmpty)
					{
						return this.remainingForwardsStack.Peek();
					}
					if (!this.remainingBackwardsStack.IsEmpty)
					{
						return this.remainingBackwardsStack.Peek();
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
			internal EnumeratorObject(ImmutableQueue<T> queue)
			{
				this.originalQueue = queue;
			}
			public bool MoveNext()
			{
				this.ThrowIfDisposed();
				if (this.remainingForwardsStack == null)
				{
					this.remainingForwardsStack = this.originalQueue.forwards;
					this.remainingBackwardsStack = this.originalQueue.BackwardsReversed;
				}
				else
				{
					if (!this.remainingForwardsStack.IsEmpty)
					{
						this.remainingForwardsStack = this.remainingForwardsStack.Pop();
					}
					else
					{
						if (!this.remainingBackwardsStack.IsEmpty)
						{
							this.remainingBackwardsStack = this.remainingBackwardsStack.Pop();
						}
					}
				}
				return !this.remainingForwardsStack.IsEmpty || !this.remainingBackwardsStack.IsEmpty;
			}
			public void Reset()
			{
				this.ThrowIfDisposed();
				this.remainingBackwardsStack = null;
				this.remainingForwardsStack = null;
			}
			public void Dispose()
			{
				this.disposed = true;
			}
			private void ThrowIfDisposed()
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException(base.GetType().FullName);
				}
			}
		}
		[ExcludeFromCodeCoverage]
		private class DebuggerProxy
		{
			private readonly ImmutableQueue<T> queue;
			private T[] contents;
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public T[] Contents
			{
				get
				{
					if (this.contents == null)
					{
						this.contents = this.queue.ToArray<T>();
					}
					return this.contents;
				}
			}
			public DebuggerProxy(ImmutableQueue<T> queue)
			{
				this.queue = queue;
			}
		}
		private static readonly ImmutableQueue<T> EmptyField = new ImmutableQueue<T>(ImmutableStack<T>.Empty, ImmutableStack<T>.Empty);
		private readonly ImmutableStack<T> backwards;
		private readonly ImmutableStack<T> forwards;
		private ImmutableStack<T> backwardsReversed;
		public bool IsEmpty
		{
			get
			{
				return this.forwards.IsEmpty && this.backwards.IsEmpty;
			}
		}
		public static ImmutableQueue<T> Empty
		{
			get
			{
				return ImmutableQueue<T>.EmptyField;
			}
		}
		private ImmutableStack<T> BackwardsReversed
		{
			get
			{
				if (this.backwardsReversed == null)
				{
					this.backwardsReversed = this.backwards.Reverse();
				}
				return this.backwardsReversed;
			}
		}
		private ImmutableQueue(ImmutableStack<T> forward, ImmutableStack<T> backward)
		{
			Requires.NotNull<ImmutableStack<T>>(forward, "forward");
			Requires.NotNull<ImmutableStack<T>>(backward, "backward");
			this.forwards = forward;
			this.backwards = backward;
			this.backwardsReversed = null;
		}
		public ImmutableQueue<T> Clear()
		{
			return ImmutableQueue<T>.Empty;
		}
		IImmutableQueue<T> IImmutableQueue<T>.Clear()
		{
			return this.Clear();
		}
		public T Peek()
		{
			if (this.IsEmpty)
			{
				throw new InvalidOperationException(Strings.InvalidEmptyOperation);
			}
			return this.forwards.Peek();
		}
		public ImmutableQueue<T> Enqueue(T value)
		{
			if (this.IsEmpty)
			{
				return new ImmutableQueue<T>(ImmutableStack<T>.Empty.Push(value), ImmutableStack<T>.Empty);
			}
			return new ImmutableQueue<T>(this.forwards, this.backwards.Push(value));
		}
		IImmutableQueue<T> IImmutableQueue<T>.Enqueue(T value)
		{
			return this.Enqueue(value);
		}
		public ImmutableQueue<T> Dequeue()
		{
			if (this.IsEmpty)
			{
				throw new InvalidOperationException(Strings.InvalidEmptyOperation);
			}
			ImmutableStack<T> immutableStack = this.forwards.Pop();
			if (!immutableStack.IsEmpty)
			{
				return new ImmutableQueue<T>(immutableStack, this.backwards);
			}
			if (this.backwards.IsEmpty)
			{
				return ImmutableQueue<T>.Empty;
			}
			return new ImmutableQueue<T>(this.BackwardsReversed, ImmutableStack<T>.Empty);
		}
		public ImmutableQueue<T> Dequeue(out T value)
		{
			value = this.Peek();
			return this.Dequeue();
		}
		IImmutableQueue<T> IImmutableQueue<T>.Dequeue()
		{
			return this.Dequeue();
		}
		public ImmutableQueue<T>.Enumerator GetEnumerator()
		{
			return new ImmutableQueue<T>.Enumerator(this);
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new ImmutableQueue<T>.EnumeratorObject(this);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ImmutableQueue<T>.EnumeratorObject(this);
		}
	}
}
