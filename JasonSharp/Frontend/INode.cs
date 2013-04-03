using System;

namespace JasonSharp.Frontend
{
	public interface INode
	{
		void Accept<T>(INodeVisitor<T> visitor, T state);
	}
}
