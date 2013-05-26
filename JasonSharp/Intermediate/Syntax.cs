using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JasonSharp.Intermediate
{
	public abstract class Node : INode
	{
		public abstract void Accept(INodeVisitor visitor);
	}

	public class AgentDeclarationNode : Node
	{
		private readonly ReadOnlyCollection<Tuple<string, string>> args;
		private readonly List<BeliefDeclarationNode> beliefDeclarations;
		private readonly List<Node> body;

		public readonly string Name;

		public IList<Tuple<string, string>> Args
		{
			get { return args; }
		}

		public IList<BeliefDeclarationNode> BeliefDeclarations
		{
			get { return beliefDeclarations; }
		}
        
		public IList<Node> Body
		{
			get { return body; }
		}
		
		public AgentDeclarationNode(string name, IList<Tuple<string, string>> args, IList<Node> body)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Tuple<string, string>>(args);
			this.beliefDeclarations = new List<BeliefDeclarationNode>();
			this.body = new List<Node>();

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

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
    
	public class BeliefDeclarationNode : Node
	{
		private readonly ReadOnlyCollection<Node> args;

		public readonly string Name;

		public IList<Node> Args
		{
			get { return args; }
		}
        
		public BeliefDeclarationNode(string name, IList<Node> args)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Node>(args);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
    
	public class HandlerDeclarationNode : Node
	{
		private readonly ReadOnlyCollection<Node> body;

		public readonly string Name;
		public IList<Node> Body
		{
			get { return body; }
		}
        
		public HandlerDeclarationNode(string name, IList<Node> body)
		{
			this.Name = name;
			this.body = new ReadOnlyCollection<Node>(body);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class PlanDeclarationNode : Node
	{
		private readonly ReadOnlyCollection<Tuple<string, string>> args;
		private readonly ReadOnlyCollection<Node> body;

		public readonly string Name;
		public IList<Tuple<string, string>> Args
		{
			get { return args; }
		}
        
		public IList<Node> Body
		{
			get { return body; }
		}
		
		public PlanDeclarationNode(string name, IList<Tuple<string, string>> args, IList<Node> body)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Tuple<string, string>>(args);
			this.body = new ReadOnlyCollection<Node>(body);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class BeliefQueryNode : Node
	{
		private readonly ReadOnlyCollection<Node> args;

		public readonly string Name;
		public IList<Node> Args
		{
			get { return args; }
		}
        
		public BeliefQueryNode(string name, IList<Node> args)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Node>(args);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class BeliefUpdateNode : Node
	{
		private readonly ReadOnlyCollection<Node> args;

		public readonly string Name;
		public IList<Node> Args
		{
			get { return args; }
		}

		public BeliefUpdateNode(string name, IList<Node> args)
		{
			this.Name = name;
			this.args = new ReadOnlyCollection<Node>(args);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class BinaryOpNode : Node
	{
		public readonly string Operator;
		public readonly Node Left;
		public readonly Node Right;

		public BinaryOpNode(string @operator, Node left, Node right)
		{
			this.Operator = @operator;
			this.Left = left;
			this.Right = right;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class IdentNode : Node
	{
		public readonly string Name;
        
		public IdentNode(string name)
		{
			this.Name = name;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}

	public class NumberNode : Node
	{
		public readonly int Value;
        
		public NumberNode(string value)
		{
			this.Value = Int32.Parse(value);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
