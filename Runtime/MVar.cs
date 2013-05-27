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

