using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace mod_oni.Base
{
	public abstract class Circuit : KMonoBehaviour
	{
		protected LogicPorts ports;

		protected override void OnSpawn(){
			try{
				this.ports = this.GetComponent<LogicPorts>();
			}
			catch{
				Debug.LogWarning($"Missing component 'LogicPorts' in {this.GetType().Name}");
			}
			
			base.Subscribe((int)GameHashes.LogicEvent, data => OnLogicNetworkConnectionChanged((LogicValueChanged)data));
		}

		private void OnLogicNetworkConnectionChanged(LogicValueChanged data){
			if(data.newValue == data.prevValue || this.GetOutputPorts().ContainsKey(data.portID)) return;
			this.OnInputPortChanged(data);
		}

		protected virtual void OnInputPortChanged(LogicValueChanged data){
			foreach(HashedString outputID in this.GetInputPorts()[data.portID]){
				try{
					int result = this.GetOutputPorts()[outputID](this.ports);
					this.ports.SendSignal(outputID, result);
				}
				catch{
					Debug.LogWarning($"Key {outputID} not found in GetOutputPorts() from {this.GetType().Name}");
				}
			}
		}
		
		public delegate int GetResult(LogicPorts port);

		/**
		 * Stores the input ports of the current circuit.
		 * It has a key and a HashSet<HashedString> that are the output ports that
		 * depends on this input value.
		 */
		public abstract Dictionary<HashedString, HashSet<HashedString>> GetInputPorts();

		/**
		 * Stores the output ports of the current circuit.
		 * Each one of these output ports must have a GetResult() callback,
		 * that defines the logical expression for the same.
		 */
		public abstract Dictionary<HashedString, GetResult> GetOutputPorts();
	}
}