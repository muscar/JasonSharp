using System;
using System.Collections.Generic;
using System.Linq;
using JasonSharp.Intermediate;

namespace JasonSharp.Frontend
{
    public class SyntacticErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public SyntacticErrorEventArgs(string message)
        {
            Message = message;
        }
    }

    public class Parser
    {
        private readonly IEnumerator<Token> tokens;

        public event EventHandler<SyntacticErrorEventArgs> ParseError;

        public bool HasErrors { get; private set; }

        public Parser(Scanner scanner)
        {
            this.tokens = scanner.Scan().GetEnumerator();
        }

        protected virtual void OnParseError(SyntacticErrorEventArgs e)
        {
            if (ParseError != null)
            {
                ParseError(this, e);
            }
            HasErrors = true;
        }

        // Utils
        
        private Token Expect(TokenKind expected, string repr)
        {
            if (tokens.Current.Kind != expected)
            {
                var message = String.Format("{0}: Expecting {1}, but got {2}", tokens.Current.Span, expected.GetDescription(), tokens.Current);
                OnParseError(new SyntacticErrorEventArgs(message));
            }
            var token = tokens.Current;
            tokens.MoveNext();
            return token;
        }

        private void SkipWhile(Predicate<Token> pred)
        {
            while (pred(tokens.Current) && tokens.MoveNext())
            {
            }
        }

        private List<T> Sequence<T>(TokenKind sep, Func<T> parser)
        {
            var seq = new List<T>() { parser() };
            while (tokens.Current.Kind == sep)
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
            Expect(TokenKind.KwAgent, "agent");
            var name = Expect(TokenKind.Ident, "agent name (identifier)");
            var args = new List<Tuple<string, string>>();
            if (tokens.Current.Kind == TokenKind.LParen)
            {
                Expect(TokenKind.LParen, "(");
                args = ParseArgumentList();
                Expect(TokenKind.RParen, ")");
            }
            Expect(TokenKind.LCurly, "{");
            var body = ParseAgentBody().ToList();
            Expect(TokenKind.RCurly, "}");
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
                        {
                            Expect(TokenKind.KwOn, "on (keyword)");
                            var handler = ParseProceduralAbstraction();
                            yield return new HandlerDeclarationNode(handler.Item1, handler.Item2, handler.Item3);
                        }
                        break;
                    case TokenKind.KwPlan:
                        {
                            Expect(TokenKind.KwPlan, "plan (keyword)");
                            var plan = ParseProceduralAbstraction();
                        yield return new PlanDeclarationNode(plan.Item1, plan.Item2, plan.Item3);
                        }
                        break;
                    default:
                        yield break;
                }
            }
        }

        private INode ParseBeliefDeclaration()
        {
            Expect(TokenKind.KwBel, "bel (keyword)");
            var name = Expect(TokenKind.Ident, "belief name (identifier)");
            var args = new List<INode>();
            Expect(TokenKind.LParen, "(");
            if (tokens.Current.Kind != TokenKind.RParen)
            {
                args = Sequence(TokenKind.Comma, ParseAtom);
            }
            Expect(TokenKind.RParen, ")");
            return new BeliefDeclarationNode(name.Contents, args);
        }

        private Tuple<string, List<Tuple<string, string>>, List<INode>> ParseProceduralAbstraction()
        {
            var name = Expect(TokenKind.Ident, "identifier");
            var args = new List<Tuple<string, string>>();
            if (tokens.Current.Kind == TokenKind.LParen)
            {
                Expect(TokenKind.LParen, "(");
                args = ParseArgumentList();
                Expect(TokenKind.RParen, ")");
            }
            Expect(TokenKind.LCurly, "{");
            var body = Sequence(TokenKind.Semicolon, ParseStatement);
            Expect(TokenKind.RCurly, "}");
            return Tuple.Create(name.Contents, args, body);
        }

        private List<Tuple<string, string>> ParseArgumentList()
        {
            Func<Tuple<string, string>> parseArg = () =>
            {
                var name = Expect(TokenKind.Ident, "argument name (identifier)");
                Expect(TokenKind.Colon, ":");
                var typeName = Expect(TokenKind.Ident, "type name");
                return Tuple.Create(name.Contents, typeName.Contents);
            };
            return Sequence(TokenKind.Comma, parseArg);
        }

        private INode ParseStatement()
        {
            switch (tokens.Current.Kind)
            {
                case TokenKind.QMark:
                    {
                        Expect(TokenKind.QMark, "?");
                        var term = ParseCompoundTerm(); 
                        return new BeliefQueryNode(term.Item1, term.Item2);
                    }
                case TokenKind.Plus:
                    {
                        Expect(TokenKind.Plus, "+");
                        var term = ParseCompoundTerm(); 
                        return new BeliefUpdateNode(term.Item1, term.Item2);
                    }
                case TokenKind.EMark:
                    {
                        Expect(TokenKind.EMark, "!");
                        var term = ParseCompoundTerm(); 
                        return new PlanInvocationNode(term.Item1, term.Item2);
                    }
                default:
                    {
                        var message = String.Format("{0}: Unexpected token: {1}", tokens.Current.Span, tokens.Current);
                        OnParseError(new SyntacticErrorEventArgs(message));
                        SkipWhile(tok => tok.Kind != TokenKind.Semicolon);
                        tokens.MoveNext();
                        return null;
                    }
            }
        }

        private Tuple<string, List<INode>> ParseCompoundTerm()
        {
            var name = Expect(TokenKind.Ident, "identifier");
            var args = new List<INode>();
            Expect(TokenKind.LParen, "(");
            if (tokens.Current.Kind != TokenKind.RParen)
            {
                args = Sequence(TokenKind.Comma, ParseExpression);
            }
            Expect(TokenKind.RParen, ")");
            return Tuple.Create(name.Contents, args);
        }

        private INode ParseExpression()
        {
            var exp = ParseTerm();
            while (tokens.Current.Kind == TokenKind.Plus || tokens.Current.Kind == TokenKind.Minus)
            {
                var op = tokens.Current.Contents;
                tokens.MoveNext();
                exp = new BinaryOpNode(op, exp, ParseExpression());
            }
            return exp;
        }

        private INode ParseTerm()
        {
            var exp = ParseAtom();
            while (tokens.Current.Kind == TokenKind.Mul || tokens.Current.Kind == TokenKind.Div)
            {
                var op = tokens.Current.Contents;
                tokens.MoveNext();
                exp = new BinaryOpNode(op, exp, ParseTerm());
            }
            return exp;
        }

        private INode ParseAtom()
        {
            var token = tokens.Current;
            switch (token.Kind)
            {
                case TokenKind.Ident:
                    tokens.MoveNext();
                    return new IdentNode(token.Contents);
                case TokenKind.Number:
                    tokens.MoveNext();
                    return new NumberNode(token.Contents);
                default:
                    {
                        var message = String.Format("{0}: Unexpected token: {1}", tokens.Current.Span, token);
                        OnParseError(new SyntacticErrorEventArgs(message));
                        tokens.MoveNext();
                        return null;
                    }
            }
        }
    }
}
