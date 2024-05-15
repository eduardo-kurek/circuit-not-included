using static mod_oni.Operation.Operator;
using static mod_oni.Parentheses.ParenthesesType;

namespace mod_oni
{
	public abstract class Token
	{
		public static implicit operator Token(string s){
			switch(s){
				case "|": return new Operation(OR);
				case "&": return new Operation(AND);
				case "^": return new Operation(XOR);
				case "~": return new Operation(NOT);
				case "(": return new Parentheses(OPENING);
				case ")": return new Parentheses(CLOSING);
				default: return new Port(s);
			}
		}
	}
}