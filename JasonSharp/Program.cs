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
