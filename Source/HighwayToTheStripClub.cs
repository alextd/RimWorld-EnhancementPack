using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(WorkGiver_Strip), "HasJobOnThing")]
	public static class HighwayToTheStripClub
    {
		//public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		public static void Postfix(Pawn pawn, Thing t, bool forced, ref bool __result)
		{
			if (!__result || !Settings.Get().stripZone) return;

			if (pawn.Map.areaManager.GetLabeled("Strip") is Area stripZone
					&& !stripZone[t.Position])
				__result = false;
		}
	}
}
