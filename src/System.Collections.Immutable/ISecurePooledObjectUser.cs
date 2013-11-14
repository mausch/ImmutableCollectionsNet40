using System;
namespace System.Collections.Immutable
{
	internal interface ISecurePooledObjectUser
	{
		Guid PoolUserId
		{
			get;
		}
	}
}
