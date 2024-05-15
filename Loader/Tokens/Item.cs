using System.Reflection.Emit;
// ReSharper disable All

namespace mod_oni
{
	public abstract class Item : Token
	{
		public abstract void Emit(ILGenerator il);
	}
}