using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
	class BillCountInventory
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return Harmony.Transpilers.MethodReplacer(instructions,
				AccessTools.Property(typeof(ThingDef), nameof(ThingDef.IsWeapon)).GetGetMethod(),
				AccessTools.Method(typeof(BillCountInventory), nameof(TrueAlwaysManWhyNot)));
		}

		public static bool TrueAlwaysManWhyNot(ThingDef d) => Settings.Get().billCountEquippedAny || d.IsWeapon;
	}

	[HarmonyPatch(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CountValidThings))]
	class FixCount
	{
		//public int CountValidThings(List<Thing> things, Bill_Production bill, ThingDef def)
		public static bool Prefix(ref int __result, RecipeWorkerCounter __instance, List<Thing> things, Bill_Production bill, ThingDef def)
		{
			//Vital fix being stackCount, not just # of things.
			__result = things.Where(t => __instance.CountValidThing(t, bill, def)).Sum(t => t.stackCount);
			return false;
		}
	}
}
