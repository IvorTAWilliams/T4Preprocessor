using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using T4Preprocessor.Models;

namespace T4Preprocessor.Services
{
	public class ParserResult
	{
		public List<Block> Blocks { get; set; } = new();
		public List<Directive> Directives { get; set; } = new();
	}
	
	public class Parser
	{
		private readonly string _content;
		private Regex _directiveStart = new Regex(@"(<#@)");
		private Regex _expressionStart = new Regex(@"(<#=)");
		private Regex _controlStart = new Regex(@"(<#( |\n|\r))");
		private Regex _classFeatureStart = new Regex(@"(<#\+)");
		private Regex _start = new Regex(@"(<#)");
		private Regex _end = new Regex(@"(#>)");

		public Parser(string content)
		{
			_content = content;
		}

		public IEnumerable<Block> GetContentBetweenMatches(IEnumerable<Match> starts, IEnumerable<Match> ends)
		{
			foreach (var start in starts)
			{
				var end = ends
					.Where(x => x.Index > start.Index)
					.OrderBy(x => x.Index)
					.FirstOrDefault();
				if (end != null)
				{
					var content = _content.Substring(start.Index + start.Length, end.Index - start.Index - start.Length);
					yield return new Block
					{
						Content = content,
						Index = start.Index + start.Length
					};
				}
			}
		}

		public ParserResult Parse()
		{
			var parserResult = new ParserResult();
			var directiveStarts = _directiveStart.Matches(_content);
			var expressionStarts = _expressionStart.Matches(_content);
			var controlStarts = _controlStart.Matches(_content);
			var classFeatureStarts = _classFeatureStart.Matches(_content);
			var starts = _start.Matches(_content);
			var ends = _end.Matches(_content);
			
			// does the file start 
			// handle mismatch start and end count
			if (starts.Count != ends.Count)
			{
				throw new Exception("Mismatch in start and end blocks.");
			}

			// handle no starts or ends
			if (starts.Count == 0 && ends.Count == 0)
			{
				parserResult.Blocks.Add(new Block
				{
					Index = 0,
					Content = _content,
					BlockType = BlockType.TextBlock
				});
				return parserResult;
			}

			var textBlocks = GetContentBetweenMatches(ends, starts).ToList();
			textBlocks.ForEach(x => x.BlockType = BlockType.TextBlock);

			// add the first text block
			var initialContent = _content.Substring(0, starts.First().Index);
			if (!string.IsNullOrEmpty(initialContent))
			{
				textBlocks.Add(new Block
				{
					Index = 0,
					Content = _content,
					BlockType = BlockType.TextBlock
				});
			}
			parserResult.Blocks.AddRange(textBlocks);

			
			var controlBlocks = GetContentBetweenMatches(controlStarts, ends).ToList();
			controlBlocks.ForEach(x => x.BlockType = BlockType.StandardControlBlock);
			parserResult.Blocks.AddRange(controlBlocks);
			
			var expressionBlocks = GetContentBetweenMatches(expressionStarts, ends).ToList();
			expressionBlocks.ForEach(x => x.BlockType = BlockType.ExpressionControlBlock);
			parserResult.Blocks.AddRange(expressionBlocks);
			
			var classFeatureBlocks = GetContentBetweenMatches(classFeatureStarts, ends).ToList();
			classFeatureBlocks.ForEach(x => x.BlockType = BlockType.ClassFeatureControlBlock);
			parserResult.Blocks.AddRange(classFeatureBlocks);
			
			var directiveBlocks = GetContentBetweenMatches(directiveStarts, ends).ToList();
			directiveBlocks.ForEach(x => x.BlockType = BlockType.Directive);
			parserResult.Directives.AddRange(directiveBlocks.Select(DirectiveFromBlock).Where(x => x != null));

			return parserResult;
		}

		public Directive DirectiveFromBlock(Block block)
		{
			var directiveType = block.Content.Trim().Split(" ").First();
			var content = block.Content.Trim().Split(" ").Last();
			if (directiveType == "import")
			{
				var importRegex = new Regex("(?<=(namespace=\")).+?(?=(\"))");
				content = importRegex.Match(content).Value;
				return new Directive {DirectiveType = DirectiveType.Import, Content = content};
			}
			return null;
		}
	}
}