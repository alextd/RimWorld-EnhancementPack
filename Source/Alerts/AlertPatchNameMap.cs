using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack.Alerts
{
	//Would like to GoTo map but Alerts don't go to maps, only the world object and that's clunky so just name it.

	class AlertPatchNameMap
	{
		public static void AddMapName(Map map, ref TaggedString report)
		{
			if (Find.Maps.Count(m => m.IsPlayerHome) == 1) return;

			if (map != null)
				report += $"\n\n({map.info.parent.Label})";
		}
	}
	[HarmonyPatch(typeof(Alert_LowFood), nameof(Alert_LowFood.GetExplanation))]
	class AlertPatchNameMap_Food
	{
		//private Map MapWithLowFood()
		public delegate Map MapWithLowFoodDel(Alert_LowFood alert);
		public static MapWithLowFoodDel MapWithLowFood = 
			AccessTools.MethodDelegate<MapWithLowFoodDel>(AccessTools.Method(typeof(Alert_LowFood), "MapWithLowFood"));

		//public virtual TaggedString GetExplanation()
		public static void Postfix(Alert_LowFood __instance, ref TaggedString __result)
		{
			AlertPatchNameMap.AddMapName(MapWithLowFood(__instance), ref __result);
		}
	}

	[HarmonyPatch(typeof(Alert_LowMedicine), nameof(Alert_LowMedicine.GetExplanation))]
	class AlertPatchNameMap_Medicine
	{
		//private Map MapWithLowMedicine()
		public delegate Map MapWithLowMedicineDel(Alert_LowMedicine alert);
		public static MapWithLowMedicineDel MapWithLowMedicine =
			AccessTools.MethodDelegate<MapWithLowMedicineDel>(AccessTools.Method(typeof(Alert_LowMedicine), "MapWithLowMedicine"));

		//public override TaggedString GetExplanation()
		public static void Postfix(Alert_LowMedicine __instance, ref TaggedString __result)
		{
			AlertPatchNameMap.AddMapName(MapWithLowMedicine(__instance), ref __result);
		}
	}

	//Would like to change Alert_NeedBatteries, Alert_NeedColonistBeds, etc
	//but they don't have GetExplanation, some of them mention the name already
}
