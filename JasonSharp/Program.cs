//
// Program.cs
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
using JasonSharp.Backend;
using JasonSharp.Frontend;

namespace JasonSharp
{
    public class JasonSharp
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: JasonSharp.exe <source>");
                return;
            }

            var compilationUnitName = Path.GetFileNameWithoutExtension(args[0]);

            var reader = new SourceReader(new StreamReader(args[0]));
            var parser = new Parser(new Scanner(reader));
            var codegen = new CodeGenerator(compilationUnitName);

            parser.ParseError += (sender, e) => Console.WriteLine(e.Message);
            codegen.CodegenError += (sender, e) => Console.WriteLine(e.Message);

            var node = parser.Parse();

            if (parser.HasErrors)
            {
                Console.WriteLine("Errors encountered during syntactic analysis. Aborting.");
                return;
            }

            codegen.Compile(node);

            if (codegen.HasErrors)
            {
                Console.WriteLine("Errors encountered during semantic analysis. Deleting assembly.");
                File.Delete(codegen.ModuleName);
                return;
            }
        }
    }
}
