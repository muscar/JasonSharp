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
