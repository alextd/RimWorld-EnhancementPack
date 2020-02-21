using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(DesignationDragger), "UpdateDragCellsIfNeeded")]
	public static class DesignationSpamKiller
	{
		public static bool Prefix(DesignationDragger __instance)
		{
			return __instance.Dragging;
		}
	}
}
