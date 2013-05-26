using System;

namespace JasonSharp.Intermediate
{
	public interface INode
	{
		void Accept(INodeVisitor visitor);
	}
}
