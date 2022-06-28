using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Mono.TextTemplating;

namespace T4Preprocessor
{
	[Verb("preprocess", HelpText = "Preprocess T4 files")]
	public class PreprocessConfiguration
	{
	}
	class Program
	{
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<PreprocessConfiguration>(args.ToList().Skip(1))
				.WithParsed<PreprocessConfiguration>(InitialiseAndRun);
		}

		static void InitialiseAndRun(PreprocessConfiguration configuration)
		{
			var directory = Directory.GetCurrentDirectory();
			Console.WriteLine($"	Processing templates for dir {directory}...");
			var nameSpace = FindNamespace(directory);
			var count = Run(directory, nameSpace);
			Console.WriteLine($"	Finished preprocessing {count} T4 templates.");
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

		public static int Run(string root, string nameSpace)
		{
			var count = 0;
			var files = Directory.GetFiles(root).Where(x => Path.GetExtension(x) == ".tt").ToList();
			var dirs = Directory.GetDirectories(root);
			foreach (var dir in dirs)
			{
				count += Run(dir, nameSpace);
			}

			var tasks = new List<Task>();
			foreach (var file in files)
			{
				tasks.Add(Task.Run(() => ConvertFile(file, nameSpace)));
			}

			count += files.Count();

			Task.WaitAll(tasks.ToArray());
			return count;
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

