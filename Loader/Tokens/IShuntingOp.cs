namespace mod_oni
{
	public interface IShuntingOp
	{
		int GetPrecedence();
		bool IsOpenParentheses();
	}
}