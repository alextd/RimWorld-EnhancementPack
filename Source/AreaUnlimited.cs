using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(AreaManager), nameof(AreaManager.CanMakeNewAllowed))]
	public static class AreaManager_CanMakeNewAllowed
	{
		static bool Prefix(ref bool __result)
		{
			if (Settings.Get().areasUnlimited)
			{
				__result = true;
				return false;
			}
			return true;
		}
	}
}
