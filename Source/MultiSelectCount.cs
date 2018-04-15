using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(InspectPaneUtility), "AdjustedLabelFor")]
	public static class MultiSelectCount
	{
		//public static string AdjustedLabelFor(IEnumerable<object> selected, Rect rect)
		public static void Postfix(ref string __result, IEnumerable<object> selected)
		{
			IEnumerable<Thing> things = selected.Where(t => t is Thing).Cast<Thing>();
			int count = things.Count();
			if (count == 0) return;

			string label = things.First().LabelCapNoCount;
			if (things.All(t => t.LabelCapNoCount == label)) return;

			if (count > 1)
				__result += " x" + count;
		}
	}
}
