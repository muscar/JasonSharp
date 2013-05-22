using System;
using System.Collections.Generic;
using System.IO;

namespace JasonSharp.Frontend
{
    public enum TokenKind
    {
        Ident,
        Number,
        
        KwAgent,
        KwBel,
        KwOn,
        KwPlan,
        KwProto,

        Plus,
        Minus,
        Mul,
        Div,
        
        LParen,
        RParen,
        LBracket,
        RBracket,
        LCurly,
        RCurly,

        Comma,
        Period,
        Semicolon,
        Colon,
        QMark,
        EMark,

        Unknown,
        Eof
    }
    
    public class Token
    {
        public static readonly Token KwAgent = new Token(TokenKind.KwAgent, "agent");
        public static readonly Token KwBel = new Token(TokenKind.KwBel, "bel");
        public static readonly Token KwOn = new Token(TokenKind.KwOn, "on");
        public static readonly Token KwPlan = new Token(TokenKind.KwPlan, "plan");
        public static readonly Token KwProto = new Token(TokenKind.KwProto, "proto");
        public static readonly Token Plus = new Token(TokenKind.Plus, "+");
        public static readonly Token Minus = new Token(TokenKind.Minus, "-");
        public static readonly Token Mul = new Token(TokenKind.Mul, "*");
        public static readonly Token Div = new Token(TokenKind.Div, "/");
        public static readonly Token LParen = new Token(TokenKind.LParen, "(");
        public static readonly Token RParen = new Token(TokenKind.RParen, ")");
        public static readonly Token LBracket = new Token(TokenKind.LBracket, "[");
        public static readonly Token RBracket = new Token(TokenKind.RBracket, "]");
        public static readonly Token LCurly = new Token(TokenKind.LCurly, "{");
        public static readonly Token RCurly = new Token(TokenKind.RCurly, "}");
        public static readonly Token Comma = new Token(TokenKind.Comma, ",");
        public static readonly Token Period = new Token(TokenKind.Period, ".");
        public static readonly Token Semicolon = new Token(TokenKind.Semicolon, ";");
        public static readonly Token Colon = new Token(TokenKind.Colon, ":");
        public static readonly Token QMark = new Token(TokenKind.QMark, "?");
        public static readonly Token EMark = new Token(TokenKind.EMark, "!");
        public static readonly Token Eof = new Token(TokenKind.Eof, "end of file");

        public readonly TokenKind Kind;
        public readonly string Contents;
        
        public Token(TokenKind kind, string contents)
        {
            this.Kind = kind;
            this.Contents = contents;
        }
        
        public override string ToString()
        {
            return Contents;
        }
    }

    public class Scanner
    {
        private readonly ISourceReader reader;
        private readonly Dictionary<string, Token> keywords = new Dictionary<string, Token>()
        {
            { "agent", Token.KwAgent },
            { "bel", Token.KwBel },
            { "on", Token.KwOn },
            { "plan", Token.KwPlan },
            { "proto", Token.KwProto }
        };

        public SourceLocation Location
        {
            get { return reader.Location; }
            private set { }
        }

        public Scanner(ISourceReader reader)
        {
            this.reader = reader;
        }
        
        public IEnumerable<Token> Scan()
        {
            while (true)
            {
                reader.ReadWhile(Char.IsWhiteSpace);
                var c = (char)reader.Peek();
                switch (c)
                {
                    case '+':
                        reader.Read();
                        yield return Token.Plus;
                        break;
                    case '-':
                        reader.Read();
                        yield return Token.Minus;
                        break;
                    case '*':
                        reader.Read();
                        yield return Token.Mul;
                        break;
                    case '/':
                        reader.Read();
                        yield return Token.Div;
                        break;
                    case '(':
                        reader.Read();                        
                        yield return Token.LParen;
                        break;
                    case ')':
                        reader.Read();
                        yield return Token.RParen;
                        break;
                    case '[':
                        reader.Read();
                        yield return Token.LBracket;
                        break;
                    case ']':
                        reader.Read();
                        yield return Token.RBracket;
                        break;
                    case '{':
                        reader.Read();
                        yield return Token.LCurly;
                        break;
                    case '}':
                        reader.Read();
                        yield return Token.RCurly;
                        break;
                    case ',':
                        reader.Read();
                        yield return Token.Comma;
                        break;
                    case '.':
                        reader.Read();
                        yield return Token.Period;
                        break;
                    case ';':
                        reader.Read();
                        yield return Token.Semicolon;
                        break;
                    case ':':
                        reader.Read();
                        yield return Token.Colon;
                        break;
                    case '?':
                        reader.Read();
                        yield return Token.QMark;
                        break;
                    case '!':
                        reader.Read();
                        yield return Token.EMark;
                        break;
                    case '\uffff':
                        yield return Token.Eof;
                        yield break;
                    default:
                        if (Char.IsDigit(c))
                        {
                            var lexeme = reader.ReadWhile(Char.IsDigit);
                            yield return new Token(TokenKind.Number, lexeme);
                        }
                        else if (Char.IsLetterOrDigit(c))
                        {
                            var lexeme = reader.ReadWhile(Char.IsLetterOrDigit);
                            
                            Token keyword;
                            if (keywords.TryGetValue(lexeme, out keyword))
                            {
                                yield return keyword;
                            }
                            else
                            {
                                yield return new Token(TokenKind.Ident, lexeme);
                            }
                        }
                        else
                        {
                            var lexeme = reader.ReadWhile(x => !Char.IsWhiteSpace(x));
                            yield return new Token(TokenKind.Unknown, lexeme);
                        }
                        break;
                }
            }
        }
    }
}
