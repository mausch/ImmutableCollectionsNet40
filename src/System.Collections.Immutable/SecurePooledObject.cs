using System;
using System.Threading;
using Validation;
namespace System.Collections.Immutable
{
	internal class SecurePooledObject<T>
	{
		internal struct SecurePooledObjectUser : IDisposable
		{
			private readonly SecurePooledObject<T> value;
			internal T Value
			{
				get
				{
					return this.value.value;
				}
			}
			internal SecurePooledObjectUser(SecurePooledObject<T> value)
			{
				this.value = value;
				Monitor.Enter(value);
			}
			public void Dispose()
			{
				Monitor.Exit(this.value);
			}
		}
		private readonly T value;
		private Guid owner;
		internal Guid Owner
		{
			get
			{
				bool flag = false;
				Guid result;
				try
				{
					Monitor.Enter(this, ref flag);
					result = this.owner;
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
				return result;
			}
			set
			{
				bool flag = false;
				try
				{
					Monitor.Enter(this, ref flag);
					this.owner = value;
				}
				finally
				{
					if (flag)
					{
						Monitor.Exit(this);
					}
				}
			}
		}
		internal SecurePooledObject(T newValue)
		{
			Requires.NotNullAllowStructs<T>(newValue, "newValue");
			this.value = newValue;
		}
		internal SecurePooledObject<T>.SecurePooledObjectUser Use<TCaller>(TCaller caller) where TCaller : ISecurePooledObjectUser
		{
			this.ThrowDisposedIfNotOwned<TCaller>(caller);
			return new SecurePooledObject<T>.SecurePooledObjectUser(this);
		}
		internal void ThrowDisposedIfNotOwned<TCaller>(TCaller caller) where TCaller : ISecurePooledObjectUser
		{
			if (caller.PoolUserId != this.owner)
			{
				throw new ObjectDisposedException(caller.GetType().FullName);
			}
		}
	}
}
