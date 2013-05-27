//
// TokenKind.cs
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

namespace JasonSharp.Frontend
{
    public enum TokenKind
    {
        // Basic lexemes
        [TokenDescription("identifier")]
        Ident,
        [TokenDescription("number")]
        Number,
        // Keywords
        [TokenDescription("agent")]
        KwAgent,
        [TokenDescription("bel")]
        KwBel,
        [TokenDescription("on")]
        KwOn,
        [TokenDescription("plan")]
        KwPlan,
        [TokenDescription("proto")]
        KwProto,
        // Operators
        [TokenDescription("+")]
        Plus,
        [TokenDescription("-")]
        Minus,
        [TokenDescription("-")]
        Mul,
        [TokenDescription("/")]
        Div,
        // Grouping
        [TokenDescription("(")]
        LParen,
        [TokenDescription(")")]
        RParen,
        [TokenDescription("[")]
        LBracket,
        [TokenDescription("]")]
        RBracket,
        [TokenDescription("{")]
        LCurly,
        [TokenDescription("}")]
        RCurly,
        // Punctuation
        [TokenDescription(",")]
        Comma,
        [TokenDescription(".")]
        Period,
        [TokenDescription(";")]
        Semicolon,
        [TokenDescription(":")]
        Colon,
        [TokenDescription("?")]
        QMark,
        [TokenDescription("!")]
        EMark,
        [TokenDescription("??")]
        QMark2,
        // Misc
        [TokenDescription("unknown")]
        Unknown,
        [TokenDescription("end of file")]
        Eof
    }
    
}
