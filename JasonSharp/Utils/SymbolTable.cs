//
// SymbolTable.cs
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

namespace JasonSharp
{
    public class SymbolTable<TKey, TVal>
    {
        private readonly Stack<Dictionary<TKey, TVal>> scopes = new Stack<Dictionary<TKey, TVal>>();

        public void EnterScope()
        {
            scopes.Push(new Dictionary<TKey, TVal>());
        }

        public void ExitScope()
        {
            scopes.Pop();
        }

        public void Register(TKey name, TVal info)
        {
            if (scopes.Count == 0)
            {
                throw new ApplicationException("Empty scope chain");
            }
            scopes.Peek().Add(name, info);
        }

        public bool TryLookup(TKey name, out TVal info)
        {
            foreach (var scope in scopes)
            {
                if (scope.TryGetValue(name, out info))
                {
                    return true;
                }
            }
            
            info = default(TVal);
            return false;
        }
    }
}
