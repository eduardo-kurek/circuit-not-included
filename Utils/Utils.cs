using System;
using System.Collections.Generic;
using STRINGS;
using TUNING;

namespace mod_oni
{
	public static class Utils
	{
		public static void AddBuildingStrings(string buildingId, string name, string description, string effect){
			Strings.Add(new string[] {
				"STRINGS.BUILDINGS.PREFABS." + buildingId.ToUpperInvariant() + ".NAME",
				UI.FormatAsLink(name, buildingId)
			});
			Strings.Add(new string[] {
				"STRINGS.BUILDINGS.PREFABS." + buildingId.ToUpperInvariant() + ".DESC",
				description
			});
			Strings.Add(new string[] {
				"STRINGS.BUILDINGS.PREFABS." + buildingId.ToUpperInvariant() + ".EFFECT",
				effect
			});
		}

		public static void AddPlan(HashedString category, string subcategory, string idBuilding, string addAfter = null){
			const string str = "Adding ";
			const string str2 = " to category ";
			var hashedString = category;
			Debug.Log(str + idBuilding + str2 + hashedString.ToString());
			foreach(var planInfo in TUNING.BUILDINGS.PLANORDER){
				if(planInfo.category == category){
					Utils.AddPlanToCategory(planInfo, subcategory, idBuilding, addAfter);
					return;
				}
			}

			Debug.Log(string.Format("Unknown build menu category: ${0}", category));
		}

		private static void AddPlanToCategory(PlanScreen.PlanInfo menu, string subcategory, string idBuilding,
			string addAfter = null){
			var buildingAndSubcategoryData = menu.buildingAndSubcategoryData;
			if(buildingAndSubcategoryData != null){
				if(addAfter == null){
					buildingAndSubcategoryData.Add(new KeyValuePair<string, string>(idBuilding, subcategory));
					return;
				}

				var num = buildingAndSubcategoryData.IndexOf(new KeyValuePair<string, string>(addAfter, subcategory));
				if(num == -1){
					Debug.Log(string.Concat(new string[] {
						"Could not find building ",
						subcategory,
						"/",
						addAfter,
						" to add ",
						idBuilding,
						" after. Adding at the end !"
					}));
					buildingAndSubcategoryData.Add(new KeyValuePair<string, string>(idBuilding, subcategory));
					return;
				}

				buildingAndSubcategoryData.Insert(num + 1, new KeyValuePair<string, string>(idBuilding, subcategory));
			}
		}

		public static void AddCategory(HashedString category, bool hideIfNotResearched, string requiredDlcId = "",
			string addAfter = null){
			var newCategory = new PlanScreen.PlanInfo(category, hideIfNotResearched, new List<string>(), requiredDlcId);
			if(addAfter == null){
				TUNING.BUILDINGS.PLANORDER.Add(newCategory);
				return;
			}

			for(var i = 0; i < TUNING.BUILDINGS.PLANORDER.Count; i++){
				if(TUNING.BUILDINGS.PLANORDER[i].category == addAfter){
					TUNING.BUILDINGS.PLANORDER.Insert(i + 1, newCategory);
					return;
				}
			}
		}
	}
}