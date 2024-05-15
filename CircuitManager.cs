using System;
using System.Collections.Generic;
using mod_oni.Base;
using mod_oni.Builders;
using mod_oni.Exceptions;
// ReSharper disable All

namespace mod_oni
{
	using InputPorts = Dictionary<HashedString, HashSet<HashedString>>;
	using OutputPorts = Dictionary<HashedString, Circuit.GetResult>;
	
	public static class CircuitManager
	{

		public class CircuitSet
		{
			public Type RegularClass;
			public Type ConfigClass;

			public CircuitSet(Type regularClass, Type configClass){
				this.RegularClass = regularClass;
				this.ConfigClass = configClass;
			}
		}
		
		private static Dictionary<string, InputPorts> INPUT_TABLE = new Dictionary<string, InputPorts>();
		private static Dictionary<string, OutputPorts> OUTPUT_TABLE = new Dictionary<string, OutputPorts>();
		private static Dictionary<string, CircuitDef> CIRCUIT_DEF_TABLE = new Dictionary<string, CircuitDef>();

		public static Dictionary<string, CircuitSet> Circuits = new Dictionary<string, CircuitSet>();

		public static bool HasCircuit(string circuitName){
			if(CircuitManager.Circuits.ContainsKey(circuitName)) return true;
			return false;
		}

		public static InputPorts GetInputPorts(string circuitName){
			if(!CircuitManager.INPUT_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot obtain {circuitName} in INPUT_TABLE", circuitName);

			var input = CircuitManager.INPUT_TABLE[circuitName];
			return CircuitManager.INPUT_TABLE[circuitName];
		}

		private static void AddInputPorts(string circuitName, InputPorts inputPorts){
			if(CircuitManager.INPUT_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"The {circuitName} circuit is already registered in INPUT_TABLE", circuitName);

			CircuitManager.INPUT_TABLE.Add(circuitName, inputPorts);
		}

		private static void RemoveInputPorts(string circuitName){
			if(!CircuitManager.INPUT_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot remove {circuitName} in INPUT_TABLE", circuitName);

			CircuitManager.INPUT_TABLE.Remove(circuitName);
		}
		
		public static OutputPorts GetOutputPorts(string circuitName){
			if(!CircuitManager.OUTPUT_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot obtain {circuitName} in OUTPUT_TABLE", circuitName);

			return CircuitManager.OUTPUT_TABLE[circuitName];
		}

		private static void AddOutputPorts(string circuitName, OutputPorts outputPorts){
			if(CircuitManager.OUTPUT_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"The {circuitName} circuit is already registered in OUTPUT_TABLE", circuitName);

			CircuitManager.OUTPUT_TABLE.Add(circuitName, outputPorts);
		}

		private static void RemoveOutputPorts(string circuitName){
			if(!CircuitManager.OUTPUT_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot remove {circuitName} in OUTPUT_TABLE", circuitName);

			CircuitManager.OUTPUT_TABLE.Remove(circuitName);
		}

		public static CircuitDef GetCircuitDef(string circuitName){
			if(!CircuitManager.CIRCUIT_DEF_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot obtain {circuitName} in CIRCUIT_DEF_TABLE", circuitName);

			return CircuitManager.CIRCUIT_DEF_TABLE[circuitName];
		}

		private static void AddCircuitDef(string circuitName, CircuitDef circuitDef){
			if(CircuitManager.CIRCUIT_DEF_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"The {circuitName} circuit is already registered in CIRCUIT_DEF_TABLE", circuitName);

			CircuitManager.CIRCUIT_DEF_TABLE.Add(circuitName, circuitDef);
		}
		
		private static void RemoveCircuitDef(string circuitName){
			if(!CircuitManager.CIRCUIT_DEF_TABLE.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot remove {circuitName} in CIRCUIT_DEF_TABLE", circuitName);

			CircuitManager.CIRCUIT_DEF_TABLE.Remove(circuitName);
		}
		
		public static CircuitSet GetCircuit(string circuitName){
			if(!CircuitManager.Circuits.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot obtain {circuitName} in Circuits", circuitName);

			return CircuitManager.Circuits[circuitName];
		}
		
		public static Dictionary<string, CircuitSet> GetCircuits(){
			return CircuitManager.Circuits;
		}

		private static void AddCircuit(string circuitName, CircuitSet circuitSet){
			if(CircuitManager.Circuits.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"The {circuitName} circuit is already registered in CIRCUITS", circuitName);

			CircuitManager.Circuits.Add(circuitName, circuitSet);
		}
		
		private static void RemoveCircuit(string circuitName){
			if(!CircuitManager.Circuits.ContainsKey(circuitName))
				throw new Exceptions.ArgumentException($"Cannot remove {circuitName} in Circuits", circuitName);

			CircuitManager.Circuits.Remove(circuitName);
		}

		private static void AddToGameAssets(CircuitSet circuit, LocString displayName, LocString description, LocString effect){
			var buildingConfig = (IBuildingConfig)Activator.CreateInstance(circuit.ConfigClass);
			BuildingDef def = buildingConfig.CreateBuildingDef();

			Utils.AddBuildingStrings(def.PrefabID, displayName, description, effect);
			ModUtil.AddBuildingToPlanScreen("Automation", def.PrefabID, "logicgates");
			BuildingConfigManager.Instance.RegisterBuilding(buildingConfig);
		}

		public static void RegisterCircuit(string name, LocString displayName, LocString description, LocString effect,
			InputPorts input, OutputPorts output, CircuitDef def)
		{
			// Inserting circuit data
			CircuitManager.AddInputPorts(name, input);
			CircuitManager.AddOutputPorts(name, output);
			CircuitManager.AddCircuitDef(name, def);
			
			// Building regular class
			CircuitBuilder circuitBuilder = new CircuitBuilder(name);
			Type regularType = circuitBuilder.Build();
			
			// Building config class
			Type circuitConfigType = typeof(CircuitConfigBuilder<>).MakeGenericType(regularType);
			Builder circuitConfigBuilder = (Builder)Activator.CreateInstance(circuitConfigType, new object[] {name});
			Type configType = circuitConfigBuilder.Build();

			// Adding circuit to the Circuits
			CircuitSet circuitSet = new CircuitSet(regularType, configType);
			CircuitManager.AddCircuit(name, circuitSet);
			
			// Adding circuit to the game assets
			CircuitManager.AddToGameAssets(circuitSet, displayName, description, effect);
		}
		
	}
}