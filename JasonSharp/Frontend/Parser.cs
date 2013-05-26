using System;
using System.Collections.Generic;
using System.Linq;
using JasonSharp.Intermediate;

namespace JasonSharp.Frontend
{
	public class ParseException : Exception
	{
		public ParseException(string message)
            : base(message)
		{
		}
	}
    
	public class Parser
	{
		private readonly IEnumerator<Token> tokens;
        
		public Parser(Scanner scanner)
		{
			this.tokens = scanner.Scan().GetEnumerator();
		}
        
		// Utils
        
		private Token Expect(Token expected)
		{
			if (tokens.Current.Kind != expected.Kind)
			{
				throw new ParseException(String.Format("Expecting `{0}`, but got `{1}`", expected, tokens.Current));
			}
			var token = tokens.Current;
			tokens.MoveNext();
			return token;
		}
        
		private List<T> Sequence<T>(Token sep, Func<T> parser)
		{
			var seq = new List<T>() { parser() };
			while (tokens.Current.Kind == sep.Kind)
			{
				tokens.MoveNext();
				seq.Add(parser());
			}
			return seq;
		}
        
		// Grammar
        
		public INode Parse()
		{
			tokens.MoveNext();
			return ParseAgent();
		}
        
		private INode ParseAgent()
		{
			Expect(Token.KwAgent);
			var name = Expect(new Token(TokenKind.Ident, "")); // XXX
			IList<Tuple<string, string>> args = new List<Tuple<string, string>>();
			if (tokens.Current.Kind == TokenKind.LParen)
			{
				Expect(Token.LParen);
				args = ParseArgumentList();
				Expect(Token.RParen);
			}
			Expect(Token.LCurly);
			var body = ParseAgentBody().ToList();
			Expect(Token.RCurly);
			return new AgentDeclarationNode(name.Contents, args, body);
		}
        
		private IEnumerable<INode> ParseAgentBody()
		{
			while (true)
			{
				switch (tokens.Current.Kind)
				{
				case TokenKind.KwBel:
					yield return ParseBeliefDeclaration();
					break;
				case TokenKind.KwOn:
					yield return ParseHandlerDeclaration();
					break;
				case TokenKind.KwPlan:
					yield return ParsePlanDeclaration();
					break;
				default:
					yield break;
				}
			}
		}
        
		private INode ParseBeliefDeclaration()
		{
			Expect(Token.KwBel);
			var name = Expect(new Token(TokenKind.Ident, "")); // XXX
			var args = new List<INode>();
			Expect(Token.LParen);
			if (tokens.Current.Kind != TokenKind.RParen)
			{
				args = Sequence(Token.Comma, ParseAtom);
			}
			Expect(Token.RParen);
			return new BeliefDeclarationNode(name.Contents, args);
		}
        
		private INode ParseHandlerDeclaration()
		{
			Expect(Token.KwOn);
			var name = Expect(new Token(TokenKind.Ident, "")); // XXX
			if (tokens.Current.Kind == TokenKind.LParen)
			{
				Expect(Token.LParen);
				var args = ParseArgumentList();
				Expect(Token.RParen);
			}
			Expect(Token.LCurly);
			var body = Sequence(Token.Semicolon, ParseStatement);
			Expect(Token.RCurly);
			return new HandlerDeclarationNode(name.Contents, body);
		}

		private INode ParsePlanDeclaration()
		{
			Expect(Token.KwPlan);
			var name = Expect(new Token(TokenKind.Ident, "")); // XXX
			IList<Tuple<string, string>> args = new List<Tuple<string, string>>();
			if (tokens.Current.Kind == TokenKind.LParen)
			{
				Expect(Token.LParen);
				args = ParseArgumentList();
				Expect(Token.RParen);
			}
			Expect(Token.LCurly);
			var body = Sequence(Token.Semicolon, ParseStatement);
			Expect(Token.RCurly);
			return new PlanDeclarationNode(name.Contents, args, body);
		}
        
		private IList<Tuple<string, string>> ParseArgumentList()
		{
			Func<Tuple<string, string>> parseArg = () =>
			{
				var name = Expect(new Token(TokenKind.Ident, "")); // XXX
				Expect(Token.Colon);
				var typeName = Expect(new Token(TokenKind.Ident, "")); // XXX
				return Tuple.Create(name.Contents, typeName.Contents);
			};
			return Sequence(Token.Comma, parseArg);
		}
        
		private INode ParseStatement()
		{
			switch (tokens.Current.Kind)
			{
                case TokenKind.QMark:
                {
                    Expect(Token.QMark);
                    var term = ParseTerm(); 
                    return new BeliefQueryNode(term.Item1, term.Item2);
                }
                case TokenKind.Plus:
                {
                    Expect(Token.Plus);
                    var term = ParseTerm(); 
                    return new BeliefUpdateNode(term.Item1, term.Item2);
                }
                case TokenKind.EMark:
                {
                    Expect(Token.EMark);
                    var term = ParseTerm(); 
                    return new PlanInvocationNode(term.Item1, term.Item2);
                }
                default:
                    throw new ParseException(String.Format("Unexpected token `{0}`", tokens.Current.Contents));
			}
		}

        private Tuple<string, List<INode>> ParseTerm()
        {
            var name = Expect(new Token(TokenKind.Ident, "")); // XXX
            var args = new List<INode>();
            Expect(Token.LParen);
            if (tokens.Current.Kind != TokenKind.RParen)
            {
                args = Sequence(Token.Comma, ParseExpression);
            }
            Expect(Token.RParen);
            return Tuple.Create(name.Contents, args);
        }

		private INode ParseExpression()
		{
			var exp = ParseAtom();
			while (tokens.Current.Kind == TokenKind.Plus || tokens.Current.Kind == TokenKind.Minus)
			{
				var op = tokens.Current.Contents;
				tokens.MoveNext();
				exp = new BinaryOpNode(op, exp, ParseExpression());
			}
			return exp;
		}
        
		private INode ParseAtom()
		{
			var token = tokens.Current;
			switch (token.Kind)
			{
			case TokenKind.Ident:
			case TokenKind.Number:
				tokens.MoveNext();
				return new IdentNode(token.Contents);
			}
			throw new ParseException(String.Format("Unexpected token `{0}`", token.Contents));
		}
	}
}
