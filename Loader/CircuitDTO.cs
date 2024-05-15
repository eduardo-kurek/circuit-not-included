// ReSharper disable All

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace mod_oni
{
	public class CellOffsetDTO
	{
		public int x;
		public int y;

		public void Validate(Validator validator, string current){
			validator.Set($"{current}.x", this.x).Required().Min(-10).Max(10);
			validator.Set($"{current}.offset.y", this.y).Required().Min(-10).Max(10);
		}
	}
	
	public class InputPortDTO
	{
		public string id;
		public CellOffsetDTO offset;
		public string description;
		public string active_description;
		public string inactive_description;
		public List<string> related_outputs;

		public void Print(){
			Debug.Log($"\tID: {id}");
			Debug.Log($"\tOFFSET: {offset.x}, {offset.y}");
			Debug.Log($"\tDESCRIPTION: {description}");
			Debug.Log($"\tACTIVE_DESCRIPTION: {active_description}");
			Debug.Log($"\tINACTIVE_DESCRIPTION: {inactive_description}");
			Debug.Log($"\tRELATED_OUTPUTS: {string.Join(", ", related_outputs)}");
		}

		public void Validate(Validator validator, string current){
			validator.Set($"{current}.id", this.id).Required().NotEmptyString();
			this.offset.Validate(validator, $"{current}.offset");
			validator.Set($"{current}.description", this.description).Required().NotEmptyString();
			validator.Set($"{current}.active_description", this.active_description).Required().NotEmptyString();
			validator.Set($"{current}.inactive_description", this.inactive_description).Required().NotEmptyString();

			for(int i = 0; i < this.related_outputs.Count; i++){
				validator.Set($"{current}.related_outputs[{i}]", this.related_outputs[i]).Required().NotEmptyString();
			}
		}
	}

	public class OutputPortDTO
	{
		public string id;
		public CellOffsetDTO offset;
		public string description;
		public string active_description;
		public string inactive_description;
		public string result;
		
		public void Print(){
			Debug.Log($"\tID: {id}");
			Debug.Log($"\tOFFSET: {offset.x}, {offset.y}");
			Debug.Log($"\tDESCRIPTION: {description}");
			Debug.Log($"\tACTIVE_DESCRIPTION: {active_description}");
			Debug.Log($"\tINACTIVE_DESCRIPTION: {inactive_description}");
			Debug.Log($"\tRESULT: {result}");
		}
		
		public void Validate(Validator validator, string current){
			validator.Set($"{current}.id", this.id).Required().NotEmptyString();
			this.offset.Validate(validator, $"{current}.offset");
			validator.Set($"{current}.description", this.description).Required().NotEmptyString();
			validator.Set($"{current}.active_description", this.active_description).Required().NotEmptyString();
			validator.Set($"{current}.result", this.result).Required().NotEmptyString();
		}
	}
	
	public class CircuitDTO
	{
		public string id;
		public string anim;
		public string display_name;
		public string description;
		public string effect;
		public int width;
		public int height;
		public List<InputPortDTO> input_ports;
		public List<OutputPortDTO> output_ports;

		public void Print(){
			Debug.Log($"ID: {id}");
			Debug.Log($"ANIM: {anim}");
			Debug.Log($"DISPLAY_NAME: {display_name}");
			Debug.Log($"DESCRIPTION: {description}");
			Debug.Log($"EFFECT: {effect}");
			Debug.Log($"WIDTH: {width}");
			Debug.Log($"HEIGHT: {height}");
			
			Debug.Log("INPUT PORTS: ");
			foreach(var input_port in input_ports){
				input_port.Print();
				Debug.Log("");
			}
			
			Debug.Log("OUTPUT PORTS: ");
			foreach(var output_port in output_ports){
				output_port.Print();
				Debug.Log("");
			}
		}

		private void ValidateId(){
			// Verifica se o ID já foi adicionado
			if(CircuitManager.HasCircuit(this.id))
				throw new Exception($"The circuit ID: {this.id} is already registered");
		}

		private void ValidateAnim(){
			// TODO
		}

		private void ValidateResult(HashSet<string> inputPorts, OutputPortDTO outputPort){
			ShuntingYard sy = new ShuntingYard(outputPort.result);
			ReversePolishNotation polish = sy.ToReversePolishNotation();

			// Análise léxica, verificando se as portas
			foreach(Item item in polish.Items){
				if(item is Port && !inputPorts.Contains(item.ToString()))
					throw new Exception($"At output_port {outputPort.id}: " +
					                    $"The input_port {item} does not exists");
			}
				
			// // Análise semântica, verificando se a expressão está correta
			// Stack<Item> result = new Stack<Item>();
			// Stack<Item> items = new Stack<Item>();
			// for(int i = polish.Items.Count - 1; i >= 0; i--) items.Push(polish.Items[i]);
			//
			// while(items.Count > 0){
			// 	switch(items.Pop()){
			// 		case Port port:
			// 			result.Push(port);
			// 			break;
			// 		
			// 		case Operation operation:
			// 			if(operation.op == Operation.Operator.NOT){
			// 				if(result.Count < 1)
			// 					throw new Exception($"Invalid result expression at output_port {outputPort.id}");
			// 			}
			// 			else{
			// 				if(result.Count < 2)
			// 					throw new Exception($"Invalid result expression at output_port {outputPort.id}");
			// 				result.Pop();
			// 			}
			// 			
			// 			break;
			// 	}
			// }
			//
			// if(result.Count != 1)
			// 	throw new Exception($"Invalid result expression at output_port {outputPort.id}");
		}

		private void ValidatePorts(){
			// Guarda os offsets para verificar se há repetidos;
			HashSet<CellOffsetDTO> offsets = new HashSet<CellOffsetDTO>();
			
			// Verifica se existe input_port repetido
			HashSet<string> inputPorts = new HashSet<string>();
			foreach(InputPortDTO inputPort in this.input_ports){
				if(inputPorts.Contains(inputPort.id))
					throw new Exception($"Duplicated input_port ID: {inputPort.id}");
				if(offsets.Contains(inputPort.offset))
					throw new Exception($"Duplicated offset in input_port: {inputPort.id}");

				offsets.Add(inputPort.offset);
				inputPorts.Add(inputPort.id);
			}
			
			// Verifica se existe output_port repetido
			HashSet<string> outputPorts = new HashSet<string>();
			foreach(OutputPortDTO outputPort in this.output_ports){
				if(outputPorts.Contains(outputPort.id))
					throw new Exception($"Duplicated output_port ID: {outputPort.id}");
				if(offsets.Contains(outputPort.offset))
					throw new Exception($"Duplicated offset in output_port: {outputPort.id}");

				offsets.Add(outputPort.offset);
				outputPorts.Add(outputPort.id);
			}
			
			// Verifica se todas as "related_output" existem
			foreach(InputPortDTO inputPort in this.input_ports){
				foreach(string port in inputPort.related_outputs){
					if(!outputPorts.Contains(port))
						throw new Exception($"The related output port (input_ports[{inputPort.id}].{port}) " +
						                    $"does not exists in output_ports");
				}
			}
			
			// Validando a expressão de todas as output_ports
			foreach(OutputPortDTO outputPort in this.output_ports){
				this.ValidateResult(inputPorts, outputPort);
			}
		}

		public void Validate(){
			Validator validator = new Validator();

			validator.Set("id", this.id).Required().NotEmptyString();
			validator.Set("anim", this.anim).Required().NotEmptyString();
			validator.Set("display_name", this.display_name).Required().NotEmptyString();
			validator.Set("description", this.description).Required().NotEmptyString();
			validator.Set("effect", this.effect).Required().NotEmptyString();
			validator.Set("width", this.width).Required().Int().Min(1).Max(10);
			validator.Set("height", this.height).Required().Int().Min(1).Max(10);
			validator.Set("input_ports", this.input_ports).Array();


			for(int i = 0; i < this.input_ports.Count; i++)
				this.input_ports[i].Validate(validator, $"input_ports[{i}]");
			for(int i = 0; i < this.output_ports.Count; i++)
				this.output_ports[i].Validate(validator, $"output_ports[{i}]");
			
			this.ValidateId();
			this.ValidateAnim();
			this.ValidatePorts();
			
		}
		
	}
}