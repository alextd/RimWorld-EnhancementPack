using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(ThingComp), nameof(ThingComp.SpecialDisplayStats))]
	public static class RottableReport
	{
		public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, ThingComp __instance)
		{
			if (__result != null)
				foreach (StatDrawEntry r in __result)
					yield return r;

			if (__instance is CompRottable compRottable)
			{
				string rotValue = ((int)compRottable.TicksUntilRotAtCurrentTemp).ToStringTicksToPeriod();

				float rotRate = GenTemperature.RotRateAtTemperature(Mathf.RoundToInt(__instance.parent.AmbientTemperature));
				if (rotRate < 0.001f)
					rotValue = "-";

				string desc = "The number of days until this item will rot when left outside.\n\nThis can be prevented by refrigerating or freezing the item.";
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Current time to rot", rotValue, desc, 1000);
			}
		}
	}

	[HarmonyPatch(typeof(CompProperties), nameof(ThingComp.SpecialDisplayStats))]
	public static class RottableReportProp
	{
		public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, CompProperties __instance)
		{
			foreach (StatDrawEntry r in __result)
				yield return r;

			if (__instance is CompProperties_Rottable propsRottable)
			{
				int rotTicks = (int)propsRottable.daysToRotStart * GenDate.TicksPerDay;
				string desc = "The number of days until this item will rot when left outside.\n\nThis can be prevented by refrigerating or freezing the item.";
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Shelf life", rotTicks.ToStringTicksToPeriod(), desc, 1000);
			}
		}
	}

	//Todo: Show inside for Plant.SpecialDisplayStats() for plant.def.plant.harvestedThingDef
}
