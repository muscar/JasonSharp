using System;

namespace JasonSharp.Frontend
{
    public interface INode
    {
        void Accept(INodeVisitor visitor);
    }
}
