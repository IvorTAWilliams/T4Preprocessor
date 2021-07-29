namespace T4Preprocessor.Models
{
	public enum DirectiveType
	{
		Import
	}
	public class Directive
	{
		public string Content { get; set; }
		public DirectiveType DirectiveType { get; set; }
	}
}