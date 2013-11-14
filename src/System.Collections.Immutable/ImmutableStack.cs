using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Validation;
namespace System.Collections.Immutable
{
	public static class ImmutableStack
	{
		public static ImmutableStack<T> Create<T>()
		{
			return ImmutableStack<T>.Empty;
		}
		public static ImmutableStack<T> Create<T>(T item)
		{
			return ImmutableStack<T>.Empty.Push(item);
		}
		public static ImmutableStack<T> CreateRange<T>(IEnumerable<T> items)
		{
			Requires.NotNull<IEnumerable<T>>(items, "items");
			ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
			foreach (T current in items)
			{
				immutableStack = immutableStack.Push(current);
			}
			return immutableStack;
		}
		public static ImmutableStack<T> Create<T>(params T[] items)
		{
			Requires.NotNull<T[]>(items, "items");
			ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
			for (int i = 0; i < items.Length; i++)
			{
				T value = items[i];
				immutableStack = immutableStack.Push(value);
			}
			return immutableStack;
		}
		public static IImmutableStack<T> Pop<T>(this IImmutableStack<T> stack, out T value)
		{
			Requires.NotNull<IImmutableStack<T>>(stack, "stack");
			value = stack.Peek();
			return stack.Pop();
		}
	}
	[DebuggerDisplay("IsEmpty = {IsEmpty}; Top = {head}"), DebuggerTypeProxy(typeof(ImmutableStack<>.DebuggerProxy))]
	public sealed class ImmutableStack<T> : IImmutableStack<T>, IEnumerable<T>, IEnumerable
	{
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public struct Enumerator
		{
			private readonly ImmutableStack<T> originalStack;
			private ImmutableStack<T> remainingStack;
			public T Current
			{
				get
				{
					if (this.remainingStack == null || this.remainingStack.IsEmpty)
					{
						throw new InvalidOperationException();
					}
					return this.remainingStack.Peek();
				}
			}
			internal Enumerator(ImmutableStack<T> stack)
			{
				Requires.NotNull<ImmutableStack<T>>(stack, "stack");
				this.originalStack = stack;
				this.remainingStack = null;
			}
			public bool MoveNext()
			{
				if (this.remainingStack == null)
				{
					this.remainingStack = this.originalStack;
				}
				else
				{
					if (!this.remainingStack.IsEmpty)
					{
						this.remainingStack = this.remainingStack.Pop();
					}
				}
				return !this.remainingStack.IsEmpty;
			}
		}
		private class EnumeratorObject : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly ImmutableStack<T> originalStack;
			private ImmutableStack<T> remainingStack;
			private bool disposed;
			public T Current
			{
				get
				{
					this.ThrowIfDisposed();
					if (this.remainingStack == null || this.remainingStack.IsEmpty)
					{
						throw new InvalidOperationException();
					}
					return this.remainingStack.Peek();
				}
			}
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			internal EnumeratorObject(ImmutableStack<T> stack)
			{
				Requires.NotNull<ImmutableStack<T>>(stack, "stack");
				this.originalStack = stack;
			}
			public bool MoveNext()
			{
				this.ThrowIfDisposed();
				if (this.remainingStack == null)
				{
					this.remainingStack = this.originalStack;
				}
				else
				{
					if (!this.remainingStack.IsEmpty)
					{
						this.remainingStack = this.remainingStack.Pop();
					}
				}
				return !this.remainingStack.IsEmpty;
			}
			public void Reset()
			{
				this.ThrowIfDisposed();
				this.remainingStack = null;
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
			private readonly ImmutableStack<T> stack;
			private T[] contents;
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public T[] Contents
			{
				get
				{
					if (this.contents == null)
					{
						this.contents = this.stack.ToArray<T>();
					}
					return this.contents;
				}
			}
			public DebuggerProxy(ImmutableStack<T> stack)
			{
				Requires.NotNull<ImmutableStack<T>>(stack, "stack");
				this.stack = stack;
			}
		}
		private static readonly ImmutableStack<T> EmptyField = new ImmutableStack<T>();
		private readonly T head;
		private readonly ImmutableStack<T> tail;
		public static ImmutableStack<T> Empty
		{
			get
			{
				return ImmutableStack<T>.EmptyField;
			}
		}
		public bool IsEmpty
		{
			get
			{
				return this.tail == null;
			}
		}
		private ImmutableStack()
		{
		}
		private ImmutableStack(T head, ImmutableStack<T> tail)
		{
			Requires.NotNull<ImmutableStack<T>>(tail, "tail");
			this.head = head;
			this.tail = tail;
		}
		public ImmutableStack<T> Clear()
		{
			return ImmutableStack<T>.Empty;
		}
		IImmutableStack<T> IImmutableStack<T>.Clear()
		{
			return this.Clear();
		}
		public T Peek()
		{
			if (this.IsEmpty)
			{
				throw new InvalidOperationException(Strings.InvalidEmptyOperation);
			}
			return this.head;
		}
		public ImmutableStack<T> Push(T value)
		{
			return new ImmutableStack<T>(value, this);
		}
		IImmutableStack<T> IImmutableStack<T>.Push(T value)
		{
			return this.Push(value);
		}
		public ImmutableStack<T> Pop()
		{
			if (this.IsEmpty)
			{
				throw new InvalidOperationException(Strings.InvalidEmptyOperation);
			}
			return this.tail;
		}
		public ImmutableStack<T> Pop(out T value)
		{
			value = this.Peek();
			return this.Pop();
		}
		IImmutableStack<T> IImmutableStack<T>.Pop()
		{
			return this.Pop();
		}
		public ImmutableStack<T>.Enumerator GetEnumerator()
		{
			return new ImmutableStack<T>.Enumerator(this);
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new ImmutableStack<T>.EnumeratorObject(this);
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ImmutableStack<T>.EnumeratorObject(this);
		}
		internal ImmutableStack<T> Reverse()
		{
			ImmutableStack<T> immutableStack = this.Clear();
			ImmutableStack<T> immutableStack2 = this;
			while (!immutableStack2.IsEmpty)
			{
				immutableStack = immutableStack.Push(immutableStack2.Peek());
				immutableStack2 = immutableStack2.Pop();
			}
			return immutableStack;
		}
	}
}
