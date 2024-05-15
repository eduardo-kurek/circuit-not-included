using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using mod_oni.Base;
// ReSharper disable All

namespace mod_oni.Builders
{
	using InputPorts = Dictionary<HashedString, HashSet<HashedString>>;
	using OutputPorts = Dictionary<HashedString, Circuit.GetResult>;
	
	public class CircuitConfigBuilder<TCircuit> : Builder where TCircuit : Circuit
	{
		private string Name;
		public Type ResultType { get; private set; }

		public CircuitConfigBuilder(string name){
			this.Name = name;
		}

		public CircuitConfigBuilder(){ }

		public void BuildMethod(TypeBuilder typeBuilder, string methodName, Type returnType, string tableMethod){
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(
				methodName,
				MethodAttributes.Public | MethodAttributes.Virtual,
				returnType,
				Type.EmptyTypes
			);

			ILGenerator il = methodBuilder.GetILGenerator();
			
			il.Emit(OpCodes.Ldstr, this.Name);
			il.Emit(OpCodes.Call, typeof(CircuitManager).GetMethod(tableMethod));
			il.Emit(OpCodes.Ret);
			
			MethodInfo originalMethod = typeof(CircuitConfig<TCircuit>).GetMethod(methodName);
			typeBuilder.DefineMethodOverride(methodBuilder, originalMethod);
		}
		
		public override Type Build(){
			AssemblyName assemblyName = new AssemblyName($"{this.Name}ConfigAssembly");
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName + "_module");

			TypeBuilder typeBuilder = moduleBuilder.DefineType($"{Name}Config", TypeAttributes.Public);
			typeBuilder.SetParent(typeof(CircuitConfig<>).MakeGenericType(typeof(TCircuit)));

			this.BuildMethod(typeBuilder, "GetCircuitDef", typeof(CircuitDef), "GetCircuitDef");

			this.ResultType = typeBuilder.CreateType();
			return this.ResultType;
		}
		
	}
}