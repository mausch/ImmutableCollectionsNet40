using System;
using Validation;
namespace System.Collections.Immutable
{
	internal class SecureObjectPool<T, TCaller> where TCaller : ISecurePooledObjectUser
	{
		private AllocFreeConcurrentStack<SecurePooledObject<T>> pool = new AllocFreeConcurrentStack<SecurePooledObject<T>>();
		public void TryAdd(TCaller caller, SecurePooledObject<T> item)
		{
			lock (item)
			{
				if (caller.PoolUserId == item.Owner)
				{
					item.Owner = Guid.Empty;
					this.pool.TryAdd(item);
				}
			}
		}
		public bool TryTake(TCaller caller, out SecurePooledObject<T> item)
		{
			if (caller.PoolUserId != Guid.Empty && this.pool.TryTake(out item))
			{
				item.Owner = caller.PoolUserId;
				return true;
			}
			item = null;
			return false;
		}
		public SecurePooledObject<T> PrepNew(TCaller caller, T newValue)
		{
			Requires.NotNullAllowStructs<T>(newValue, "newValue");
			return new SecurePooledObject<T>(newValue)
			{
				Owner = caller.PoolUserId
			};
		}
	}
}
