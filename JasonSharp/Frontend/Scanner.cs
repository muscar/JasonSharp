using System;
using System.Collections.Generic;

using JasonSharp.Text;

namespace JasonSharp.Frontend
{    
    public class Token
    {
        public TokenKind Kind { get; private set; }
        public string Contents { get; private set; }
        public TextSpan Span { get; private set; }
        
        public Token(TokenKind kind, string contents, TextSpan span)
        {
            Kind = kind;
            Contents = contents;
            Span = span;
        }

        public override string ToString()
        {
            return Contents;
        }
    }

    public class Scanner
    {
        private readonly SourceReader reader;
        private readonly Dictionary<string, TokenKind> keywords = new Dictionary<string, TokenKind>
        {
            { "agent", TokenKind.KwAgent },
            { "bel", TokenKind.KwBel },
            { "on", TokenKind.KwOn },
            { "plan", TokenKind.KwPlan },
            { "proto", TokenKind.KwProto }
        };

        public TextLocation Position
        {
            get { return reader.Location; }
        }

        public Scanner(SourceReader reader)
        {
            this.reader = reader;
        }
        
        public IEnumerable<Token> Scan()
        {
            while (true)
            {
                reader.ReadWhile(Char.IsWhiteSpace);
                var startLocation = reader.Location;
                var lexeme = String.Empty;
                var tokenKind = TokenKind.Unknown;
                var c = (char) reader.Peek();

                switch (c)
                {
                    case '+':
                        reader.Read();
                        tokenKind = TokenKind.Plus;
                        lexeme = "+";
                        break;
                    case '-':
                        reader.Read();
                        tokenKind = TokenKind.Minus;
                        lexeme = "-";
                        break;
                    case '*':
                        reader.Read();
                        tokenKind = TokenKind.Mul;
                        lexeme = "*";
                        break;
                    case '/':
                        reader.Read();
                        tokenKind = TokenKind.Div;
                        lexeme = "/";
                        break;
                    case '(':
                        reader.Read();                        
                        tokenKind = TokenKind.LParen;
                        lexeme = "(";
                        break;
                    case ')':
                        reader.Read();
                        tokenKind = TokenKind.RParen;
                        lexeme = ")";
                        break;
                    case '[':
                        reader.Read();
                        tokenKind = TokenKind.LBracket;
                        lexeme = "[";
                        break;
                    case ']':
                        reader.Read();
                        tokenKind = TokenKind.RBracket;
                        lexeme = "]";
                        break;
                    case '{':
                        reader.Read();
                        tokenKind = TokenKind.LCurly;
                        lexeme = "{";
                        break;
                    case '}':
                        reader.Read();
                        tokenKind = TokenKind.RCurly;
                        lexeme = "}";
                        break;
                    case ',':
                        reader.Read();
                        tokenKind = TokenKind.Comma;
                        lexeme = ",";
                        break;
                    case '.':
                        reader.Read();
                        tokenKind = TokenKind.Period;
                        lexeme = ".";
                        break;
                    case ';':
                        reader.Read();
                        tokenKind = TokenKind.Semicolon;
                        lexeme = ";";
                        break;
                    case ':':
                        reader.Read();
                        tokenKind = TokenKind.Colon;
                        lexeme = ":";
                        break;
                    case '?':
                        reader.Read();
                        if (reader.Peek() == '?')
                        {
                            reader.Read();
                            tokenKind = TokenKind.QMark2;
                            lexeme = "??";
                        }
                        else
                        {
                            tokenKind = TokenKind.QMark;
                            lexeme = "?";
                        }
                        break;
                    case '!':
                        reader.Read();
                        tokenKind = TokenKind.EMark;
                        lexeme = "!";
                        break;
                    case '\uffff':
                        tokenKind = TokenKind.Eof;
                        lexeme = "end of file";
                        yield break;
                    default:
                        if (Char.IsDigit(c))
                        {
                            lexeme = reader.ReadWhile(Char.IsDigit);
                            tokenKind = TokenKind.Number;
                        }
                        else if (Char.IsLetterOrDigit(c))
                        {
                            lexeme = reader.ReadWhile(Char.IsLetterOrDigit);
                            
                            if (!keywords.TryGetValue(lexeme, out tokenKind))
                            {
                                tokenKind = TokenKind.Ident;
                            }
                        }
                        else
                        {
                            lexeme = reader.ReadWhile(x => !Char.IsWhiteSpace(x));
                        }
                        break;
                }

                yield return new Token(tokenKind, lexeme, new TextSpan(startLocation, reader.Location));
            }
        }
    }
}
