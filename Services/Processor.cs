using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using T4Preprocessor.Models;

namespace T4Preprocessor.Services
{
	public class Processor
	{
		private readonly ParserResult _parserResult;
		private readonly string _fileName;
		private readonly string _nameSpace;
		private readonly StringBuilder _sb;
		private List<string> _tabs;

		public Processor(ParserResult parserResult, string fileName, string nameSpace)
		{
			_parserResult = parserResult;
			_fileName = fileName;
			_nameSpace = nameSpace;
			_sb = new StringBuilder();
			_tabs = new();
		}

		private void Write(string content)
		{
			_sb.Append(content);
		}

		private void WriteLine(string content)
		{
			_tabs.ForEach(x => _sb.Append(x));
			_sb.AppendLine(content);
		}

		private void AddTab() => _tabs.Add("\t");
		private void RemoveTab() => _tabs.RemoveAt(0);

		private void WriteTextBlock(string content)
		{
			var lineEndings = (new Regex(@"\r\n|\n|\r"))
				.Matches(content)
				.OrderBy(x => x.Index)
				.ToList();
			if (!lineEndings.Any())
			{
				WriteLine($"sb.Append(\"{content}\")");
				return;
			}
			
			// there is onle a single new line (between two control blocks) igore
			if (lineEndings.Count == 1 && content.Length == lineEndings[0].Length)
			{
				return;
			}

			if (lineEndings.First().Index != 0)
			{
				var section = content.Substring(0, lineEndings.First().Index);
				WriteLine($"sb.Append(\"{section}\")");
				WriteLine($"sb.AppendLine()");
			}

			for (var i = 0; i < lineEndings.Count - 1; i++)
			{
				var start = lineEndings[i];
				var end = lineEndings[i + 1];
				var section = content.Substring(start.Index + start.Length, end.Index - start.Index - start.Length);
				WriteLine($"sb.Append(\"{section}\")");
				WriteLine($"sb.AppendLine()");
			}

			// write the last part of the line if there is one
			var last = lineEndings.Last();
			if (last.Index + last.Length != content.Length)
			{
				var section = content.Substring(last.Index + last.Length, content.Length - last.Index - last.Length);
				WriteLine($"sb.Append(\"{section}\")");
			}
		}

		private void WriteExpression(string content)
		{
			WriteLine($"sb.Append({content.Trim()}.ToString())");
		}

		private void WriteControlBlock(string content)
		{
			WriteLine(content);
		}

		public string Process()
		{
			_parserResult.Directives
				.Where(x => x.DirectiveType == DirectiveType.Import).ToList()
				.ForEach(x => WriteLine($"using {x.Content};"));
			WriteLine($"namespace {_nameSpace}");
			WriteLine("{");
			AddTab();
			WriteLine($"public partial class {_fileName}");
			WriteLine("{");
			AddTab();
			_parserResult.Blocks
				.Where(x => x.BlockType == BlockType.ClassFeatureControlBlock)
				.ToList()
				.ForEach(x => WriteLine(x.Content));
			WriteLine($"public string TransformText()");
			WriteLine("{");
			AddTab();
			WriteLine("var sb = new StringBuilder();");

			var blocks = _parserResult.Blocks.OrderBy(x => x.Index);
			foreach (var block in blocks)
			{
				switch (block.BlockType)
				{
					case BlockType.TextBlock:
						WriteTextBlock(block.Content);
						break;
					case BlockType.ExpressionControlBlock:
						WriteExpression(block.Content);
						break;
					case BlockType.StandardControlBlock:
						WriteControlBlock(block.Content);
						break;
				}
			}
			WriteLine("return sb.ToString();");
			RemoveTab();
			WriteLine("}");
			RemoveTab();
			WriteLine("}");
			RemoveTab();
			WriteLine("}");
			return FormatContents(_sb.ToString());
		}

		public string FormatContents(string contents)
		{
			var tabs = new List<string>();
			var sb = new StringBuilder();
			var lines = contents.Split(Environment.NewLine);
			foreach (var line in lines)
			{
				// remove old tabbing
				var trimmedLine = line.Trim();

				if (string.IsNullOrEmpty(trimmedLine))
				{
					sb.AppendLine();
					continue;
				}
				
				// check brackets
				if (trimmedLine == "}")
				{
					tabs.RemoveAt(0);
				}
				
				// apply new tabbing
				tabs.ForEach(x => sb.Append(x));
				sb.Append(trimmedLine);
				
				// check brackets
				if (trimmedLine == "{" || trimmedLine.Last() == '{')
				{
					tabs.Add("\t");
				}

				sb.AppendLine();
			}
			return sb.ToString();
		}
	}
}