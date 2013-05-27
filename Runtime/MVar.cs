//
// Token.cs
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
using System.Threading;

namespace Runtime
{
    public class MVar<T>
    {
        private readonly object valueLock = new object();
        private bool full = false;
        private T value;
        
        public bool Empty
        {
            get
            {
                lock (valueLock)
                {
                    return !full;
                }
            }
        }
        
        public MVar(T value)
        {
            this.value = value;
            this.full = true;
        }
        
        public T Take()
        {
            lock (valueLock)
            {
                while (!full)
                {
                    Monitor.Wait(valueLock);
                }
                full = false;
                Monitor.PulseAll(valueLock);
                return value;
            }
        }
        
        public void Put(T value)
        {
            lock (valueLock)
            {
                while (full)
                {
                    Monitor.Wait(valueLock);
                }
                this.value = value;
                this.full = true;
                Monitor.PulseAll(valueLock);
            }
        }
        
        public bool TryTake(ref T value)
        {
            lock (valueLock)
            {
                if (full)
                {
                    full = false;
                    value = this.value;
                }
                return !full;
            }
        }
        
        public bool TryPut(T value)
        {
            lock (valueLock)
            {
                if (!full)
                {
                    full = true;
                    this.value = value;
                }
                return full;
            }
        }
        
        public T Read()
        {
            lock (valueLock)
            {
                if (!full)
                {
                    return default(T);
                }
                return value;
            }
        }
    }
}

