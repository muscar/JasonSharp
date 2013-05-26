using System;

namespace JasonSharp.Intermediate
{
	public interface INodeVisitor
	{
		void Visit(AgentDeclarationNode node);
		void Visit(BeliefDeclarationNode node);
		void Visit(HandlerDeclarationNode node);
		void Visit(PlanDeclarationNode node);
        void Visit(PlanInvocationNode node);
		void Visit(BeliefQueryNode node);
		void Visit(BeliefUpdateNode node);
		void Visit(BinaryOpNode node);
		void Visit(IdentNode node);
		void Visit(NumberNode node);
	}
}
