//
// SourceReader.cs
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
using System.IO;
using System.Text;
using JasonSharp.Text;

namespace JasonSharp
{
    public class SourceReader
    {
        private readonly TextReader reader;
        private readonly StringBuilder buffer;
        private int offset;
        private int line = 1;
        private int column;

        public bool EndOfStream
        {
            get
            {
                return reader.Peek() == -1;
            }
        }

        public TextLocation Location
        {
            get
            {
                return new TextLocation(offset, line, column);
            }
        }

        public SourceReader(TextReader reader)
        {
            this.reader = reader;
            this.buffer = new StringBuilder();
        }

        public virtual char Peek()
        {
            return (char) reader.Peek();
        }

        public virtual char Read()
        {
            var c = (char) reader.Read();
            // This will work do an extra column++ on Windows style newline, but
            // it's harmless since it will immediately be followed by a \n so
            // the column count will get resetted.
            if (c == '\n')
            {
                column = 0;
                line++;
            }
            else
            {
                column++;
            }
            offset++;
            return c;
        }

        public string ReadWhile(Predicate<char> pred)
        {
            buffer.Clear();
            while (!EndOfStream && pred(Peek()))
            {
                buffer.Append(Read());
            }
            return buffer.ToString();
        }
    }
}

