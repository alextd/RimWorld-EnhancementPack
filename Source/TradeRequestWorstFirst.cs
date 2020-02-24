using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(CaravanInventoryUtility), nameof(CaravanInventoryUtility.TakeThings))]
	class TradeRequestWorstFirst
	{
		//public static List<Thing> TakeThings(Caravan caravan, Func<Thing, int> takeQuantity)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo ToListInfo = AccessTools.Method(typeof(Enumerable), "ToList").MakeGenericMethod(typeof(Thing));

			foreach(CodeInstruction i in instructions)
			{
				yield return i;

				if(i.Calls(ToListInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TradeRequestWorstFirst), nameof(SortedByValue)));
				}
			}
		}

		public static List<Thing> SortedByValue(List<Thing> list)
		{
			if(Settings.Get().tradeRequestWorstFirst)
				list.SortBy(t => t.MarketValue);
			return list;
		}
	}
}
