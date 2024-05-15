using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using mod_oni.Base;
using mod_oni;
using STRINGS;

// ReSharper disable All

namespace mod_oni.Builders
{
	using InputPorts = Dictionary<HashedString, HashSet<HashedString>>;
	using OutputPorts = Dictionary<HashedString, Circuit.GetResult>;
	
	public class CircuitBuilder : Builder
	{
		
		private string name;
		public Type ResultType { get; private set; }

		public CircuitBuilder(string name){
			this.name = name;
		}

		
		private void BuildMethod(TypeBuilder typeBuilder, string methodName, Type returnType, string tableMethod){
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(
				methodName,
				MethodAttributes.Public | MethodAttributes.Virtual,
				returnType,
				Type.EmptyTypes
			);
			
			ILGenerator il = methodBuilder.GetILGenerator();
			
			il.Emit(OpCodes.Ldstr, this.name);
			il.Emit(OpCodes.Call, typeof(CircuitManager).GetMethod(tableMethod));
			il.Emit(OpCodes.Ret);
			
			MethodInfo originalMethod = typeof(Circuit).GetMethod(methodName);
			typeBuilder.DefineMethodOverride(methodBuilder, originalMethod);
		}

		public override Type Build(){
			AssemblyName assemblyName = new AssemblyName($"{this.name}Assembly");
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName + "_module");

			TypeBuilder typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public, typeof(Circuit));
			
			BuildMethod(typeBuilder, "GetInputPorts", typeof(InputPorts), "GetInputPorts");
			BuildMethod(typeBuilder, "GetOutputPorts", typeof(OutputPorts), "GetOutputPorts");
			
			this.ResultType = typeBuilder.CreateType();
			return this.ResultType;
		}
		
	}
}