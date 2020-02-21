using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Area_Home), "Set")]
	public static class NeverHomeArea
	{
		//protected virtual void Set(IntVec3 c, bool val)
		public static bool Prefix(Area_Home __instance, IntVec3 c, bool val)
		{
			if (!Settings.Get().neverHome) return true;

			if (val
				&& __instance.Map.areaManager.GetLabeled("Never Home") is Area neverHome
				&& neverHome[c])
				return false;
			return true;
		}
	}
}
