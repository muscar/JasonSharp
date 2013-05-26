using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JasonSharp.Intermediate
{
	public class AgentDeclarationNode : INode
	{
		private readonly ReadOnlyCollection<Tuple<string, string>> args;
		private readonly List<BeliefDeclarationNode> beliefDeclarations;
		private readonly List<INode> body;

		public readonly string Name;

		public IList<Tuple<string, string>> Args
		{
			get { return args; }
		}

		public IEnumerable<BeliefDeclarationNode> BeliefDeclarations
		{
			get { return beliefDeclarations; }
		}
        
		public IEnumerable<INode> Body
		{
			get { return body; }
		}
		
		public AgentDeclarationNode(string name, IList<Tuple<string, string>> args, IList<INode> body)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Tuple<string, string>>(args);
			this.beliefDeclarations = new List<BeliefDeclarationNode>();
			this.body = new List<INode>();

			foreach (var node in body)
			{
				var belief = node as BeliefDeclarationNode;
				if (belief != null)
				{
					beliefDeclarations.Add(belief);
				}
				else
				{
					this.body.Add(node);
				}
			}
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
    
	public class BeliefDeclarationNode : INode
	{
		private readonly ReadOnlyCollection<INode> args;

		public readonly string Name;

		public IEnumerable<INode> Args
		{
			get { return args; }
		}
        
		public BeliefDeclarationNode(string name, IList<INode> args)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<INode>(args);
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
    
	public class HandlerDeclarationNode : INode
	{
		private readonly ReadOnlyCollection<INode> body;

		public readonly string Name;
		public IList<INode> Body
		{
			get { return body; }
		}
        
		public HandlerDeclarationNode(string name, IList<INode> body)
		{
			this.Name = name;
			this.body = new ReadOnlyCollection<INode>(body);
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class PlanDeclarationNode : INode
	{
		private readonly ReadOnlyCollection<Tuple<string, string>> args;
		private readonly ReadOnlyCollection<INode> body;

		public readonly string Name;
		public IList<Tuple<string, string>> Args
		{
			get { return args; }
		}
        
		public IList<INode> Body
		{
			get { return body; }
		}
		
		public PlanDeclarationNode(string name, IList<Tuple<string, string>> args, IList<INode> body)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Tuple<string, string>>(args);
			this.body = new ReadOnlyCollection<INode>(body);
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

    public abstract class PlanActionNode
    {
        private readonly ReadOnlyCollection<INode> args;

        public string Name { get; private set; }
        public IList<INode> Args { get { return args; } }

        public PlanActionNode(string name, IList<INode> args)
        {
            this.Name = name;
            this.args = new ReadOnlyCollection<INode>(args);
        }
    }

    public class PlanInvocationNode : PlanActionNode, INode
    {
        public PlanInvocationNode(string name, IList<INode> args) : base(name, args) { }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BeliefQueryNode : PlanActionNode, INode
	{
		public BeliefQueryNode(string name, IList<INode> args) : base(name, args) { }

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

    public class BeliefUpdateNode : PlanActionNode, INode
    {
        public BeliefUpdateNode(string name, IList<INode> args) : base(name, args) { }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

	public class BinaryOpNode : INode
	{
		public readonly string Operator;
		public readonly INode Left;
		public readonly INode Right;

		public BinaryOpNode(string @operator, INode left, INode right)
		{
			this.Operator = @operator;
			this.Left = left;
			this.Right = right;
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class IdentNode : INode
	{
		public readonly string Name;
        
		public IdentNode(string name)
		{
			this.Name = name;
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class NumberNode : INode
	{
		public readonly int Value;
        
		public NumberNode(string value)
		{
			this.Value = Int32.Parse(value);
		}

		public void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
