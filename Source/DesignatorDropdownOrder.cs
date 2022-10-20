using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Designator_Dropdown), "Add")]
	public static class DesignatorDropdownOrder
	{
		//public void Add(Designator des)
		public static void Postfix(Designator_Dropdown __instance, List<Designator> ___elements)
		{
			__instance.Order = ___elements.Sum(d => d.Order) / ___elements.Count();
		}
	}
}
