## Base classes

Firstly i created the two classes that i'll need to abstract the circuits, the regular class, and the IBuildingConfig class, see below:

Regular class:
````csharp
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

	private GetResult getResult;

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
````

The way it works is simple. When any of the circuit input ports changes, we send a signal to every output port influenced by this input. For each output that receives this signal, we obtain the result and adjust the output value accordingly.

The result would be the formula for that output port, for example ((i1 + i2) * i3).

Config class:
````csharp
public class CircuitDef
{
	public string id;
	public int width;
	public int height;
	public string anim;
	public List<LogicPorts.Port> inputPorts;
	public List<LogicPorts.Port> outputPorts;
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
````

<hr>

## Dynamic building

In order to make the IL code of the dynamic classes easier, i'll store the data of these 3 abstracts methods and list in wich circuit they belongs:
````csharp
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
}
````

For creating these classes in runtime, i'll need a auxiliar class for each one, that i called them Builders:
`````csharp
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
`````

````csharp
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
````

Putting all of this in the `CircuitManager`, it looks like:

````csharp
public static class CircuitManager
{
	
	...
		
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
````

<hr>

## Creating a circuit

In the `'Main'` method, i manually wrote the circuit data, but these will not be hardcoded.

````csharp
[HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
public class GeneratingLogicGateNewAnd
{
	public static void Prefix(){
		string name = "LogicGateNewAnd";

		var inputPorts = new Dictionary<HashedString, HashSet<HashedString>>() {
			{ "i1", new HashSet<HashedString> { "o1" } },
			{ "i2", new HashSet<HashedString> { "o1" } },
			{ "i3", new HashSet<HashedString> { "o1" } }
		};

		var outputPorts = new Dictionary<HashedString, GetResult>() {
			{
				"o1",
				ports => { return (ports.GetInputValue("i1") | ports.GetInputValue("i2")) & ports.GetInputValue("i3"); }
			}
		};

		CircuitDef circuitDef = new CircuitDef(
			name, 2, 2, "logic_new_and_kanim",
			new List<LogicPorts.Port>() {
				LogicPorts.Port.InputPort("i1", new CellOffset(0, 1),
					"i1", "i1", "i1"),
				LogicPorts.Port.InputPort("i2", new CellOffset(0, 0),
					"i2", "i2", "i2"),
				LogicPorts.Port.InputPort("i3", new CellOffset(0, -1),
					"i3", "i3", "i3")
			},
			new List<LogicPorts.Port>() {
				LogicPorts.Port.OutputPort("o1", new CellOffset(1, 0),
					"o1", "o1", "o1"),
			}
		);

		CircuitManager.RegisterCircuit(name, "DISPLAY NAME", "DESCRIPTION", "EFFECT", inputPorts, outputPorts,
			circuitDef);
	}
}
````