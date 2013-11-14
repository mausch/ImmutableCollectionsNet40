using System;
using System.Collections.Generic;
using System.Threading;
using Validation;
namespace System.Collections.Immutable
{
	public static class ImmutableInterlocked
	{
		public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
		{
			Requires.NotNull<Func<TKey, TArg, TValue>>(valueFactory, "valueFactory");
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
			TValue tValue;
			if (immutableDictionary.TryGetValue(key, out tValue))
			{
				return tValue;
			}
			tValue = valueFactory(key, factoryArgument);
			return ImmutableInterlocked.GetOrAdd<TKey, TValue>(ref location, key, tValue);
		}
		public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> valueFactory)
		{
			Requires.NotNull<Func<TKey, TValue>>(valueFactory, "valueFactory");
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
			TValue tValue;
			if (immutableDictionary.TryGetValue(key, out tValue))
			{
				return tValue;
			}
			tValue = valueFactory(key);
			return ImmutableInterlocked.GetOrAdd<TKey, TValue>(ref location, key, tValue);
		}
		public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
		{
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			TValue result;
			while (true)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
				if (immutableDictionary.TryGetValue(key, out result))
				{
					break;
				}
				ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Add(key, value);
				ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, value2, immutableDictionary);
				bool flag = object.ReferenceEquals(immutableDictionary, immutableDictionary2);
				immutableDictionary = immutableDictionary2;
				if (flag)
				{
					return value;
				}
			}
			return result;
		}
		public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
		{
			Requires.NotNull<Func<TKey, TValue>>(addValueFactory, "addValueFactory");
			Requires.NotNull<Func<TKey, TValue, TValue>>(updateValueFactory, "updateValueFactory");
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			TValue tValue;
			bool flag;
			do
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
				TValue arg;
				if (immutableDictionary.TryGetValue(key, out arg))
				{
					tValue = updateValueFactory(key, arg);
				}
				else
				{
					tValue = addValueFactory(key);
				}
				ImmutableDictionary<TKey, TValue> value = immutableDictionary.SetItem(key, tValue);
				ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, value, immutableDictionary);
				flag = object.ReferenceEquals(immutableDictionary, immutableDictionary2);
				immutableDictionary = immutableDictionary2;
			}
			while (!flag);
			return tValue;
		}
		public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
		{
			Requires.NotNull<Func<TKey, TValue, TValue>>(updateValueFactory, "updateValueFactory");
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			TValue tValue;
			bool flag;
			do
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
				TValue arg;
				if (immutableDictionary.TryGetValue(key, out arg))
				{
					tValue = updateValueFactory(key, arg);
				}
				else
				{
					tValue = addValue;
				}
				ImmutableDictionary<TKey, TValue> value = immutableDictionary.SetItem(key, tValue);
				ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, value, immutableDictionary);
				flag = object.ReferenceEquals(immutableDictionary, immutableDictionary2);
				immutableDictionary = immutableDictionary2;
			}
			while (!flag);
			return tValue;
		}
		public static bool TryAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
		{
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			while (true)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
				if (immutableDictionary.ContainsKey(key))
				{
					break;
				}
				ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Add(key, value);
				ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, value2, immutableDictionary);
				bool flag = object.ReferenceEquals(immutableDictionary, immutableDictionary2);
				immutableDictionary = immutableDictionary2;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
		public static bool TryUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue)
		{
			EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			while (true)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
				TValue x;
				if (!immutableDictionary.TryGetValue(key, out x) || !@default.Equals(x, comparisonValue))
				{
					break;
				}
				ImmutableDictionary<TKey, TValue> value = immutableDictionary.SetItem(key, newValue);
				ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, value, immutableDictionary);
				bool flag = object.ReferenceEquals(immutableDictionary, immutableDictionary2);
				immutableDictionary = immutableDictionary2;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
		public static bool TryRemove<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, out TValue value)
		{
			ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read<ImmutableDictionary<TKey, TValue>>(ref location);
			while (true)
			{
				Requires.NotNull<ImmutableDictionary<TKey, TValue>>(immutableDictionary, "location");
				if (!immutableDictionary.TryGetValue(key, out value))
				{
					break;
				}
				ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Remove(key);
				ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, value2, immutableDictionary);
				bool flag = object.ReferenceEquals(immutableDictionary, immutableDictionary2);
				immutableDictionary = immutableDictionary2;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
		public static bool TryPop<T>(ref ImmutableStack<T> location, out T value)
		{
			ImmutableStack<T> immutableStack = Volatile.Read<ImmutableStack<T>>(ref location);
			while (true)
			{
				Requires.NotNull<ImmutableStack<T>>(immutableStack, "location");
				if (immutableStack.IsEmpty)
				{
					break;
				}
				ImmutableStack<T> value2 = immutableStack.Pop(out value);
				ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange<ImmutableStack<T>>(ref location, value2, immutableStack);
				bool flag = object.ReferenceEquals(immutableStack, immutableStack2);
				immutableStack = immutableStack2;
				if (flag)
				{
					return true;
				}
			}
			value = default(T);
			return false;
		}
		public static void Push<T>(ref ImmutableStack<T> location, T value)
		{
			ImmutableStack<T> immutableStack = Volatile.Read<ImmutableStack<T>>(ref location);
			bool flag;
			do
			{
				Requires.NotNull<ImmutableStack<T>>(immutableStack, "location");
				ImmutableStack<T> value2 = immutableStack.Push(value);
				ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange<ImmutableStack<T>>(ref location, value2, immutableStack);
				flag = object.ReferenceEquals(immutableStack, immutableStack2);
				immutableStack = immutableStack2;
			}
			while (!flag);
		}
		public static bool TryDequeue<T>(ref ImmutableQueue<T> location, out T value)
		{
			ImmutableQueue<T> immutableQueue = Volatile.Read<ImmutableQueue<T>>(ref location);
			while (true)
			{
				Requires.NotNull<ImmutableQueue<T>>(immutableQueue, "location");
				if (immutableQueue.IsEmpty)
				{
					break;
				}
				ImmutableQueue<T> value2 = immutableQueue.Dequeue(out value);
				ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange<ImmutableQueue<T>>(ref location, value2, immutableQueue);
				bool flag = object.ReferenceEquals(immutableQueue, immutableQueue2);
				immutableQueue = immutableQueue2;
				if (flag)
				{
					return true;
				}
			}
			value = default(T);
			return false;
		}
		public static void Enqueue<T>(ref ImmutableQueue<T> location, T value)
		{
			ImmutableQueue<T> immutableQueue = Volatile.Read<ImmutableQueue<T>>(ref location);
			bool flag;
			do
			{
				Requires.NotNull<ImmutableQueue<T>>(immutableQueue, "location");
				ImmutableQueue<T> value2 = immutableQueue.Enqueue(value);
				ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange<ImmutableQueue<T>>(ref location, value2, immutableQueue);
				flag = object.ReferenceEquals(immutableQueue, immutableQueue2);
				immutableQueue = immutableQueue2;
			}
			while (!flag);
		}
	}
}
