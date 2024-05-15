using System.Reflection.Emit;
// ReSharper disable All

namespace mod_oni
{
	public class Operation : Item, IShuntingOp
	{

		public enum Operator
		{
			AND,
			OR,
			XOR,
			NOT
		}

		public Operator op;
	
		public Operation(Operator op){
			this.op = op;
		}
		
		public OpCode GetCode(){
			switch (this.op){
				case Operator.AND: return OpCodes.And;
				case Operator.OR: return OpCodes.Or;
				case Operator.XOR: return OpCodes.Xor;
				default: return OpCodes.Not;
			}
		}

		public int GetPrecedence(){
			switch (this.op){
				case Operator.NOT: return 4;
				case Operator.AND: return 3;
				case Operator.XOR: return 2;
				case Operator.OR: return 1;
				default: return 0;
			}
		}

		public bool IsOpenParentheses(){
			return false;
		}

		// Pushes the operation onto the stack
		public override void Emit(ILGenerator il){
			il.Emit(this.GetCode());
		}

		public override string ToString(){
			switch (this.op){
				case Operator.NOT: return "NOT";
				case Operator.AND: return "AND";
				case Operator.XOR: return "XOR";
				case Operator.OR: return "OR";
				default: return "NULL";
			}
		}
	}
}