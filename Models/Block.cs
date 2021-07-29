namespace T4Preprocessor.Models
{
	public enum BlockType
	{
		TextBlock,
		StandardControlBlock,
		ExpressionControlBlock,
		Directive,
		ClassFeatureControlBlock
	}
	public class Block
	{
		public int Index { get; set; }
		public string Content { get; set; }
		public BlockType BlockType { get; set; }
	}
}