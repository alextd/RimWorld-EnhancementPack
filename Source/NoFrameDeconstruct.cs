using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Designator_Deconstruct), "CanDesignateThing")]
	public static class NoFrameDeconstruct
	{
		//public override AcceptanceReport CanDesignateThing(Thing t)
		public static void Postfix(ref AcceptanceReport __result, Thing t, Designator_Deconstruct __instance)
		{
			if (!__result.Accepted) return;

			if (t is Frame)
				__result = false;
		}
	}
}
