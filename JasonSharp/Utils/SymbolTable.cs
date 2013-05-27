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

        public TVal LookupAs(TKey name)
		{
			foreach (var scope in scopes)
			{
				TVal info;
				if (scope.TryGetValue(name, out info))
				{
					return info;
				}
			}
			
			throw new ApplicationException(String.Format("`{0}` is not in scope", name));
		}

        public TResult LookupAs<TResult>(TKey name)
            where TResult : class, TVal
		{
			foreach (var scope in scopes)
			{
				TVal info;
				if (scope.TryGetValue(name, out info))
				{
                    var result = info as TResult;
					if (result != null)
					{
						return result;
					}
                    throw new ApplicationException(String.Format("`{0}` is {1}, but it's used as {2}", name, typeof(TVal).Name, typeof(TResult).Name));
				}
			}

			throw new ApplicationException(String.Format("`{0}` is not in scope", name));
		}
	}
}
