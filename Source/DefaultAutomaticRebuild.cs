using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(PlaySettings), MethodType.Constructor)]
	public static class DefaultAutomaticRebuild
	{
		public static void Postfix(PlaySettings __instance)
		{
			__instance.autoRebuild = true;
		}
	}
}
