using System.Collections.Generic;
using TUNING;
using UnityEngine;
// ReSharper disable All

namespace mod_oni.Base
{
	public class CircuitDef
	{
		public string id;
		public int width;
		public int height;
		public string anim;
		public List<LogicPorts.Port> inputPorts;
		public List<LogicPorts.Port> outputPorts;

		public CircuitDef(string id, int width, int height, string anim, List<LogicPorts.Port> inputPorts, List<LogicPorts.Port> outputPorts){
			this.id = id;
			this.width = width;
			this.height = height;
			this.anim = anim;
			this.inputPorts = inputPorts;
			this.outputPorts = outputPorts;
		}
	}

	public abstract class CircuitConfig<TCircuit> : IBuildingConfig where TCircuit : Circuit
	{
		public override BuildingDef CreateBuildingDef(){
			var def = this.GetCircuitDef();

			var hitpoints = 30;
			var construction_time = 30f;
			var tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
			var all_METALS = MATERIALS.REFINED_METALS;
			var melting_point = 1600f;
			var build_location_rule = BuildLocationRule.Anywhere;
			var none = NOISE_POLLUTION.NONE;
			
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				def.id, def.width, def.height, def.anim, hitpoints, construction_time, tier,
				all_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, none, 0.2f
			);

			buildingDef.ViewMode = OverlayModes.Logic.ID;
			buildingDef.ObjectLayer = ObjectLayer.LogicGate;
			buildingDef.SceneLayer = Grid.SceneLayer.LogicGates;
			buildingDef.ThermalConductivity = 0.05f;
			buildingDef.Floodable = false;
			buildingDef.Overheatable = false;
			buildingDef.Entombable = false;
			buildingDef.AudioCategory = "Metal";
			buildingDef.AudioSize = "small";
			buildingDef.BaseTimeUntilRepair = -1f;
			buildingDef.PermittedRotations = PermittedRotations.R360;
			buildingDef.DragBuild = true;
			buildingDef.LogicInputPorts = def.inputPorts;
			buildingDef.LogicOutputPorts = def.outputPorts;

			GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, def.id);
			return buildingDef;
		}

		public override void DoPostConfigureComplete(GameObject go){
			go.AddOrGet<LogicPorts>();
			go.AddComponent<TCircuit>();
		}

		public abstract CircuitDef GetCircuitDef();
	}
}