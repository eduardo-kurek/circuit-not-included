using System.Reflection.Emit;

namespace mod_oni
{
	public class Port : Item
	{
		public string PortId;

		public Port(string portId){
			this.PortId = portId;
		}

		// Calls the ports.GetInputValue(this.portId) and pushes its result onto the stack
		public override void Emit(ILGenerator il){
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldstr, this.PortId);
			il.Emit(OpCodes.Newobj, typeof(HashedString).GetConstructor(new []{typeof(string)}));
			il.Emit(OpCodes.Callvirt, typeof(LogicPorts).GetMethod("GetInputValue"));
		}

		public override string ToString(){
			return this.PortId;
		}
	}
}