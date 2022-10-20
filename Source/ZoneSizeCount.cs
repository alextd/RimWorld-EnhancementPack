using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TD_Enhancement_Pack
{

	[HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.GetInspectString))]
	public static class ZoneGrowingSizeCount
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// After getting base.GetInspectString(), append Fertility Count
			//	IL_0001: call instance string Verse.Zone::GetInspectString()
			MethodInfo GetInspectStringInfo = AccessTools.Method(typeof(Zone), nameof(Zone.GetInspectString));

			foreach (var inst in instructions)
			{
				yield return inst;

				if (inst.Calls(GetInspectStringInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//this
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ZoneGrowingSizeCount), nameof(AppendFertileCount)));//AppendFertileCount(string, this)
				}
			}
		}

		public static string AppendFertileCount(string baseString, Zone_Growing zone)
		{
			if (!Mod.settings.showGrowingFertilitySize) return baseString;

			float fertCount = zone.CellCount + 
				zone.GetPlantDefToGrow().plant.fertilitySensitivity
				* zone.cells.Sum(cell => zone.Map.fertilityGrid.FertilityAt(cell) - 1.0f);
			return $"{baseString} ({"TD.FertileCount".Translate()}: {fertCount:0.0})";
		}
	}
}
