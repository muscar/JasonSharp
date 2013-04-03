using System;
using System.Collections.Generic;

namespace JasonSharp
{
	public class SymbolTable<T>
	{
		private readonly Stack<Dictionary<string, T>> scopes = new Stack<Dictionary<string, T>>();

		public void Enter()
		{
			scopes.Push(new Dictionary<string, T>());
		}

		public void Exit()
		{
			scopes.Pop();
		}

		public void Register(string name, T info)
		{
			if (scopes.Count == 0)
			{
				throw new ApplicationException("Empty scope chain");
			}
			scopes.Peek().Add(name, info);
		}

		public T Lookup(string name)
		{
			foreach (var scope in scopes)
			{
				T info;
				if (scope.TryGetValue(name, out info))
				{
					return info;
				}
			}
			
			throw new ApplicationException(String.Format("`{0}` is not in scope", name));
		}

		public U Lookup<U>(string name)
			where U : class, T
		{
			foreach (var scope in scopes)
			{
				T info;
				if (scope.TryGetValue(name, out info))
				{
					var result = info as U;
					if (result != null)
					{
						return result;
					}
					throw new ApplicationException(String.Format("`{0}` is {1}, but it's used as {2}", name, typeof(T).Name, typeof(U).Name));
				}
			}

			throw new ApplicationException(String.Format("`{0}` is not in scope", name));
		}
	}
}
