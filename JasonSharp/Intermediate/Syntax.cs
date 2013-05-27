//
// Syntax.cs
//
// Author:
//       Alex Muscar <muscar@gmail.com>
//
// Copyright (c) 2013 Alex Muscar
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ArgumentList = System.Collections.Generic.List<System.Tuple<string, string>>;

namespace JasonSharp.Intermediate
{
    #region Abstract base classes

    public abstract class AbstractProcedureNode
    {
        protected List<Tuple<string, string>> ArgsList { get; private set; }

        protected List<INode> BodyList { get; private set; }

        public string Name { get; private set; }

        public ReadOnlyCollection<Tuple<string, string>> Args
        {
            get { return ArgsList.AsReadOnly(); }
        }

        public ReadOnlyCollection<INode> Body
        {
            get { return BodyList.AsReadOnly(); }
        }

        public AbstractProcedureNode(string name, List<Tuple<string, string>> args, List<INode> body)
        {
            Name = name;
            ArgsList = args;
            BodyList = body;
        }
    }
    public abstract class AbstractActionNode
    {
        private readonly List<INode> args;

        public string Name { get; private set; }

        public ReadOnlyCollection<INode> Args { get { return args.AsReadOnly(); } }

        public AbstractActionNode(string name, List<INode> args)
        {
            this.Name = name;
            this.args = args;
        }
    }

    #endregion

    public class AgentDeclarationNode : AbstractProcedureNode, INode
    {
        private readonly List<BeliefDeclarationNode> beliefDeclarations;

        public ReadOnlyCollection<BeliefDeclarationNode> BeliefDeclarations
        {
            get { return beliefDeclarations.AsReadOnly(); }
        }

        public AgentDeclarationNode(string name, ArgumentList args, List<INode> body)
            : base(name, args, new List<INode>())
        {
            this.beliefDeclarations = new List<BeliefDeclarationNode>();

            foreach (var node in body)
            {
                var belief = node as BeliefDeclarationNode;
                if (belief != null)
                {
                    beliefDeclarations.Add(belief);
                }
                else
                {
                    BodyList.Add(node);
                }
            }
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BeliefDeclarationNode : AbstractActionNode, INode
    {
        public BeliefDeclarationNode(string name, List<INode> args)
            : base(name, args)
        {
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class HandlerDeclarationNode : AbstractProcedureNode, INode
    {
        public HandlerDeclarationNode(string name, ArgumentList args, List<INode> body)
            : base(name, args, body)
        {
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class PlanDeclarationNode : AbstractProcedureNode, INode
    {
        public PlanDeclarationNode(string name, ArgumentList args, List<INode> body)
            : base(name, args, body)
        {
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class PlanInvocationNode : AbstractActionNode, INode
    {
        public PlanInvocationNode(string name, List<INode> args) : base(name, args)
        {
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BeliefQueryNode : AbstractActionNode, INode
    {
        public BeliefQueryNode(string name, List<INode> args) : base(name, args)
        {
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BeliefUpdateNode : AbstractActionNode, INode
    {
        public BeliefUpdateNode(string name, List<INode> args) : base(name, args)
        {
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BinaryOpNode : INode
    {
        public string Operator { get; private set; }
        public INode Left { get; private set; }
        public INode Right { get; private set; }

        public BinaryOpNode(string op, INode left, INode right)
        {
            this.Operator = op;
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
        public string Name { get; private set; }

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
        public int Value { get; private set; }

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
