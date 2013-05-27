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
			var foo = new Test(10, 20);
			foo.bar(3);
			Console.WriteLine(foo.foo);
			return;

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
			var node = parser.Parse();

            if (parser.HasErrors)
            {
                Console.WriteLine("Errors encountered while parsing");
                return;
            }

            codegen.Compile(node);
		}
	}
}
