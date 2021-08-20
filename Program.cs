using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.TextTemplating;

namespace T4Preprocessor
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1 || !Directory.Exists(args[0]))
			{
				return;
			}

			var nameSpace = FindNamespace(args[0]);
			
			Run(args[0], nameSpace);
		}

		public static string FindNamespace(string root)
		{
			var csproj = Directory.GetFiles(root).FirstOrDefault(x => Path.GetExtension(x) == ".csproj");
			if (csproj != null)
			{
				return Path.GetFileNameWithoutExtension(csproj);
			}
			return FindNamespace(Directory.GetParent(root)?.FullName);
		}

		public static void Run(string root, string nameSpace)
		{
			var files = Directory.GetFiles(root).Where(x => Path.GetExtension(x) == ".tt");
			var dirs = Directory.GetDirectories(root);
			foreach (var dir in dirs)
			{
				Run(dir, nameSpace);
			}
			var tasks = new List<Task>();
			foreach (var file in files)
			{
				tasks.Add(Task.Run(() => ConvertFile(file, nameSpace)));
			}
			Task.WaitAll(tasks.ToArray());
		}
		
		public static void ConvertFile(string inputFileName, string nameSpace)
		{
			var outputFileName = inputFileName.Replace(".tt", ".cs");
			var className = Path.GetFileName(inputFileName).Replace(".", "_").Replace("-", "_");

			var generator = new TemplateGenerator();
			generator.PreprocessTemplate(
				inputFileName,
				$"{className}_{Guid.NewGuid().ToString().Replace("-", "_")}",
				nameSpace,
				File.ReadAllText(inputFileName),
				out var lang,
				out var refs,
				out var contents);
			
			File.WriteAllText(outputFileName, contents);
		}
	}
}