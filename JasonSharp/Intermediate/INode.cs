using System;

namespace JasonSharp.Intermediate
{
	public interface INode
	{
		void Accept<T>(INodeVisitor<T> visitor, T state);
	}
}
