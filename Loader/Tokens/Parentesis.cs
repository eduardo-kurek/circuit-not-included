// ReSharper disable All

namespace mod_oni
{
	public class Parentheses : Token, IShuntingOp
	{
		public enum ParenthesesType
		{
			OPENING,
			CLOSING
		}

		private ParenthesesType parenthesesType;

		public Parentheses(ParenthesesType parenthesesType){
			this.parenthesesType = parenthesesType;
		}

		public ParenthesesType GetType(){
			return this.parenthesesType;
		}

		public int GetPrecedence(){
			return 0;
		}

		public bool IsOpenParentheses(){
			return this.parenthesesType == ParenthesesType.OPENING;
		}
	}
}