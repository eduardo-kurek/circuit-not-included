using System.Collections.Generic;
using System.Reflection.Emit;
// ReSharper disable All

namespace mod_oni
{
	public class ReversePolishNotation
	{
		public List<Item> Items { get; private set; }

		public ReversePolishNotation(List<Item> items){
			this.Items = items;
		}
		
		public void Emit(ILGenerator il){  
			foreach(Item item in this.Items) item.Emit(il);
			il.Emit(OpCodes.Ret);
		}
		
	}

}