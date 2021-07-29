using System;
using System.IO;
using T4Preprocessor.Services;

namespace T4Preprocessor
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1 || !File.Exists(args[0]))
			{
				return;
			}

			var contents = File.ReadAllText(args[0]);
			var parser = new Parser(contents);
			var result = parser.Parse();
			var processor = new Processor(result, "Test", "T4Generator");
			var output = processor.Process();
		}
	}
}