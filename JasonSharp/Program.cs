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
//			var foo = new Test(10, 20);
//			foo.bar(1);
//			Console.WriteLine(foo.foo);
//			return;
			using (var reader = new SourceStreamReader(@"../../examples/foo.mj"))
			{
				var node = new Parser(new Scanner(reader)).Parse();
				CodeGenerator.Compile("foo", node);
			}
		}
	}
}
