//
// Scanner.cs
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
using JasonSharp.Text;

namespace JasonSharp.Frontend
{
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
            while (!reader.EndOfStream)
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

            yield return new Token(TokenKind.Eof, "end of file", new TextSpan(reader.Location, reader.Location));
        }
    }
}
