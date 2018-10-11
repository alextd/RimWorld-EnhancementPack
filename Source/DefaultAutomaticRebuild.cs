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
			if(Settings.Get().autorebuildDefaultOn)
				__instance.autoRebuild = true;
		}
	}
}
