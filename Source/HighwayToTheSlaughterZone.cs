using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(WorkGiver_Slaughter), "HasJobOnThing")]
	public static class HighwayToTheSlaughterZone
	{
		//public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		public static void Postfix(Pawn pawn, Thing t, bool forced, ref bool __result)
		{
			if (!__result || !Mod.settings.slaughterZone) return;

			if (pawn.Map.areaManager.GetLabeled("Slaughter") is Area slaughterZone
					&& !slaughterZone[t.Position])
				__result = false;
		}
	}
}
