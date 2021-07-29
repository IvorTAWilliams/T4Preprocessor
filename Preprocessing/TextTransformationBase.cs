using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace T4Preprocessor.Preprocessing
{
	public class TextTransformationBase
	{
		public StringBuilder GenerationEnvironment { get; } = new StringBuilder();
		public string CurrentIndent => _currentIndent;

		private Stack<string> Indents { get; } = new Stack<string>();
		private string _currentIndent = String.Empty;
		private bool _endWithNewLine = true;

		public void PushIndent(string indent)
		{
			Indents.Push(indent);
			_currentIndent += indent;
		}

		public string PopIndent()
		{
			if (Indents.Any())
			{
				var indent = Indents.Pop();
				_currentIndent = _currentIndent.Substring(0, _currentIndent.Length - indent.Length);
				return indent;
			}

			return null;
		}

		public void ClearIndent()
		{
			Indents.Clear();
			_currentIndent = String.Empty;
		}

		public void Write(string textToAppend)
		{
			// clear new lines
			Regex lineEndings = new Regex(@"\r\n|\n|\r");
			textToAppend = lineEndings.Replace(textToAppend, Environment.NewLine + _currentIndent);

			// indent new line
			if (_endWithNewLine)
			{
				GenerationEnvironment.Append(_currentIndent);
			}

			if (textToAppend.EndsWith(Environment.NewLine + _currentIndent))
			{
				textToAppend = textToAppend.Substring(0,
					textToAppend.Length - _currentIndent.Length);
				_endWithNewLine = true;
			}
			else
			{
				_endWithNewLine = false;
			}

			GenerationEnvironment.Append(textToAppend);
		}

		public void WriteLine(string textToAppend = "")
		{
			Write(textToAppend);
			GenerationEnvironment.AppendLine();
			_endWithNewLine = true;
		}

		public virtual string TransformText()
		{
			return "";
		}
	}
}