﻿using System;
using System.IO;
using JasonSharp.Backend;
using JasonSharp.Frontend;

namespace JasonSharp
{
	public class JasonSharp
	{
		public static void Main(string[] args)
		{
//			var foo = new Test(1, 2);
//			foo.bar(1);
//			Console.WriteLine(foo.foo);
//			return;
			using (var reader = new StreamReader(@"../../examples/foo.mj"))
			{
				var node = new Parser(new Scanner(reader)).Parse();
				var codeGenerator = new CodeGenerator("foo");
				node.Accept(codeGenerator);
				codeGenerator.Bake();
			}
		}
	}
}
