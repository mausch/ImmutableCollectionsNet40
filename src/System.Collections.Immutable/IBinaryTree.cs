using System;
namespace System.Collections.Immutable
{
	internal interface IBinaryTree<out T>
	{
		int Height
		{
			get;
		}
		T Value
		{
			get;
		}
		IBinaryTree<T> Left
		{
			get;
		}
		IBinaryTree<T> Right
		{
			get;
		}
		bool IsEmpty
		{
			get;
		}
		int Count
		{
			get;
		}
	}
}
