using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Zone_Growing))]
	[HarmonyPatch("GetInspectString")]
	static class ZoneGrowingSizeCount
	{
		public static void Postfix(Zone_Growing __instance, ref string __result)
		{
			//TODO save __instance.cells.Sum(cell => __instance.Map.fertilityGrid.FertilityAt(cell))

			//TODO: call Zone.GetInspectString (base)
			__result += String.Format("TD.SizeAndFertile".Translate(), 
				__instance.cells.Count,
				__instance.cells.Sum(cell => __instance.Map.fertilityGrid.FertilityAt(cell)));
		}
	}

	[HarmonyPatch(typeof(Zone))]
	[HarmonyPatch("GetInspectString")]
	static class ZoneSizeCount
	{
		public static void Postfix(Zone __instance, ref string __result)
		{
			string add = String.Format("TD.Size".Translate(), __instance.cells.Count);
			if (__result == String.Empty)
				__result = add;
			else
				__result += "\n" + add;
		}
	}
}
