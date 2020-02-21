using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(GenPath), "ShouldNotEnterCell")]
	public static class QuickGrab
	{
		//private static bool ShouldNotEnterCell(Pawn pawn, Map map, IntVec3 dest)
		public static void Postfix(ref bool __result, Pawn pawn, Map map, IntVec3 dest)
		{
			if (__result || !Settings.Get().quickGrab) return;

			if (dest.GetEdifice(map) is Building_Storage)
				__result = true;
		}
	}
}
