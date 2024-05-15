using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json;
using HarmonyLib;
using Microsoft.CSharp;
using mod_oni.Base;
using static mod_oni.Base.Circuit;
using static mod_oni.Operation.Operator;

namespace mod_oni
{
	public class Patches
	{
		
		[HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
		public class GeneratingLogicGateNewAnd
		{

			public static void Prefix(){
				//
				// var inputPorts = new Dictionary<HashedString, HashSet<HashedString>>(){
				// 	{ "i1", new HashSet<HashedString> { "o1" } },
				// 	{ "i2", new HashSet<HashedString> { "o1" } },
				// 	{ "i3", new HashSet<HashedString> { "o1" } }
				// };
				//
				// DynamicMethod dynamicMethod = new DynamicMethod("as", typeof(int), new[] { typeof(LogicPorts) });
				// ILGenerator il = dynamicMethod.GetILGenerator();
				//
				// var items = new List<Item>() {
				// 	new Port("i1"),
				// 	new Port("i2"),
				// 	new Operation(OR),
				// 	new Port("i3"),
				// 	new Operation(AND)
				// };
				//
				// ReversePolishNotation rpn = new ReversePolishNotation(items);
				// rpn.Emit(il);
				//
				// var func = (Func<LogicPorts, int>)dynamicMethod.CreateDelegate(typeof(Func<LogicPorts, int>));
				//
				// var outputPorts = new Dictionary<HashedString, GetResult>() {
				// 	{
				// 		"o1",
				// 		new GetResult(func)
				// 	}
				// };
				//
				// CircuitDef circuitDef = new CircuitDef(
				// 	name, 2, 2, "logic_new_and_kanim",
				// 	new List<LogicPorts.Port>(){
				// 		LogicPorts.Port.InputPort("i1", new CellOffset(0, 1), 
				// 			"i1", "i1", "i1"),
				// 		LogicPorts.Port.InputPort("i2", new CellOffset(0, 0), 
				// 			"i2", "i2", "i2"),
				// 		LogicPorts.Port.InputPort("i3", new CellOffset(0, -1), 
				// 			"i3", "i3", "i3")
				// 	},
				// 	new List<LogicPorts.Port>() {
				// 		LogicPorts.Port.OutputPort("o1", new CellOffset(1, 0), 
				// 			"o1", "o1", "o1"),
				// 	}
				// );
				//
				// CircuitManager.RegisterCircuit(name, "DISPLAY NAME", "DESCRIPTION", "EFFECT",
				// 	inputPorts, outputPorts, circuitDef);
				string path = "C:\\Users\\eduar\\Desktop\\working\\circuit-not-included\\my_first_circuit.json";
				string content = File.ReadAllText(path);
				
				try{
					CircuitDTO cdto = JsonConvert.DeserializeObject<CircuitDTO>(content);
					//cdto.Print();
					//cdto.Validate();
					
					// 1° parte: Montando portas
					var inputPorts = new Dictionary<HashedString, HashSet<HashedString>>();
					var outputPorts = new Dictionary<HashedString, GetResult>();
					foreach(InputPortDTO input in cdto.input_ports){
						var relatedOutputs = new HashSet<HashedString>();
						
						foreach(string relatedOutput in input.related_outputs)
							relatedOutputs.Add(relatedOutput);
						
						inputPorts.Add(input.id, relatedOutputs);
					}

					foreach(OutputPortDTO output in cdto.output_ports){
						DynamicMethod dynamicMethod = new DynamicMethod($"{output.id}_GetResult", 
							typeof(int), new[] { typeof(LogicPorts) });
						ILGenerator il = dynamicMethod.GetILGenerator();

						// Emitindo o código do método dinamico
						string expression = output.result;
						ShuntingYard sy = new ShuntingYard(expression);
						ReversePolishNotation rpn = sy.ToReversePolishNotation();
						rpn.Emit(il);
					
						var func = (Func<LogicPorts, int>)dynamicMethod.CreateDelegate(typeof(Func<LogicPorts, int>));
						
						// Registrando a porta de saída
						outputPorts.Add(output.id, new GetResult(func));
					}
					
					// 2° parte: Criando CircuitDef
					CircuitDef circuitDef = new CircuitDef(
						cdto.id, cdto.width, cdto.height, cdto.anim,
						new List<LogicPorts.Port>(),
						new List<LogicPorts.Port>()
					);

					foreach(InputPortDTO input in cdto.input_ports){
						LogicPorts.Port port = LogicPorts.Port.InputPort(
							input.id, new CellOffset(input.offset.x, input.offset.y),
							input.description, input.active_description, input.inactive_description
						);
						circuitDef.inputPorts.Add(port);
					}

					foreach(OutputPortDTO output in cdto.output_ports){
						LogicPorts.Port port = LogicPorts.Port.OutputPort(
							output.id, new CellOffset(output.offset.x, output.offset.y),
							output.description, output.active_description, output.inactive_description
						);
						circuitDef.outputPorts.Add(port);
					}

					CircuitManager.RegisterCircuit(cdto.id, cdto.display_name, cdto.description, cdto.effect,
						inputPorts, outputPorts, circuitDef);

				}
				catch(Exception e){
					Debug.LogError(e.Message);
				}
				

				
			}
			
		}
	}
}