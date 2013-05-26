using System;

namespace JasonSharp.Intermediate
{
	public interface INodeVisitor<T>
	{
		void Visit(AgentDeclarationNode node, T state);
		void Visit(BeliefDeclarationNode node, T state);
		void Visit(HandlerDeclarationNode node, T state);
		void Visit(PlanDeclarationNode node, T state);
		void Visit(BeliefQueryNode node, T state);
		void Visit(BeliefUpdateNode node, T state);
		void Visit(BinaryOpNode node, T state);
		void Visit(IdentNode node, T state);
		void Visit(NumberNode node, T state);
	}
}
