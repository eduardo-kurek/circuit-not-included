To create a new item in the game you need the following things:

1. The game needs a class that derives  the `IBuildingConfig` class. This class describes the item definition, like the width, heigh, sounds, materials, recipe, logic ports, construction rules, overlay tab, etc.. This alone is enough for the item to be registered in the game. See the example:

```csharp
public class ManualGeneratorConfig : IBuildingConfig
{
	public override BuildingDef CreateBuildingDef()
	{
		string id = "ManualGenerator";
		int width = 2;
		int height = 2;
		string anim = "generatormanual_kanim";
		int hitpoints = 30;
		float construction_time = 30f;
		float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
		string[] all_METALS = MATERIALS.ALL_METALS;
		float melting_point = 1600f;
		BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
		EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER3;
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, all_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, tier2, 0.2f);
		buildingDef.GeneratorWattageRating = 400f;
		buildingDef.GeneratorBaseCapacity = 10000f;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.Breakable = true;
		buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingFront;
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.SelfHeatKilowattsWhenActive = 1f;
		return buildingDef;
	}
}
```

Once this class is written and compiled, the game code will look for all classes in the `AppDomain` assembly that inherts `IBuildingConfig` and add the same to the game assets. More specifically in the `LegacyModMain.Load()` method.

But if we want to be able to add a specific behaviour and logic to our item, we need to:

2. Create a class that inherits from `KMonoBehaviour` and just write your own code. In it, you can override any method from `KMonoBehaviour` that you want for your goals. See the example:

```csharp
public class ManualGenerator : Workable
{
	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.SetWorkTime(float.PositiveInfinity);
		KBatchedAnimController component = base.GetComponent<KBatchedAnimController>();
		foreach (KAnimHashedString symbol in ManualGenerator.symbol_names)
		{
			component.SetSymbolVisiblity(symbol, false);
		}
		Building component2 = base.GetComponent<Building>();
		this.powerCell = component2.GetPowerOutputCell();
		this.OnActiveChanged(null);
		this.overrideAnims = new KAnimFile[]
		{
			Assets.GetAnim("anim_interacts_generatormanual_kanim")
		};
		this.smi = new ManualGenerator.GeneratePowerSM.Instance(this);
		this.smi.StartSM();
		Game.Instance.energySim.AddManualGenerator(this);
	}
}
```

Figure out that in this case, the Workable class inherits from `KMonoBehaviour`, making this piece of code valid in our context. Just note that there are many other details behind this `OnSpawn()` method wich i've hidden here. Under the covers, this class will serve as a component for the `IBuildingConfig` GameObject that we have already seen above.

3. Now we need to add this newly created component to the `IBuildingConfig` class, and we can do it like this:

```csharp
public class ManualGeneratorConfig : IBuildingConfig
{
	public override BuildingDef CreateBuildingDef()
	{
		...
	}
    
	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddComponent<ManualGenerator>();
	}
}
```

You can verify when the `DoPostConfigureComplete()` is called by checking `BuildingConfigManager.RegisterBuilding(IBuildingConfig)`.

4. To register the new item to some building category, you'll need to execute the following code:

`````csharp
Utils.AddBuildingStrings(ITEM_ID, DISPLAY_NAME, DESCRIPTION, EFFECT);
ModUtil.AddBuildingToPlanScreen(CATEGORY_ID, ITEM_ID, SUBCATEGORY_ID, RELATIVE_TO);
`````

In the first line, we'll adding the informative strings of the item.
Above, we'll registering the item in some category, and we also can pass a relative position (it'll be inserted after the relative).