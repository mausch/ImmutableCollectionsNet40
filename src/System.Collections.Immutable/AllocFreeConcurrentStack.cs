using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace System.Collections.Immutable
{
	[DebuggerDisplay("Count = {stack.Count}")]
	internal class AllocFreeConcurrentStack<T>
	{
		private readonly Stack<RefAsValueType<T>> stack = new Stack<RefAsValueType<T>>();
		public void TryAdd(T item)
		{
			lock (this.stack)
			{
				this.stack.Push(new RefAsValueType<T>(item));
			}
		}
		public bool TryTake(out T item)
		{
			lock (this.stack)
			{
				int count = this.stack.Count;
				if (count > 0)
				{
					item = this.stack.Pop().Value;
					return true;
				}
			}
			item = default(T);
			return false;
		}
	}
}
