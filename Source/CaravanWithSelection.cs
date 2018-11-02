using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_FormCaravan), "CalculateAndRecacheTransferables")]
	static class CaravanWithSelection
	{
		//private void CalculateAndRecacheTransferables()
		public static void Postfix(Dialog_FormCaravan __instance)
		{
			if (!Settings.Get().caravanLoadSelection) return;

			foreach (object obj in Find.Selector.SelectedObjectsListForReading.Where(o => o is Thing))
				if (obj is Thing thing)
				{
					Log.Message($"adding {thing}:{thing.stackCount}");
					TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thing, __instance.transferables, TransferAsOneMode.PodsOrCaravanPacking);
					transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thing.stackCount));
				}
		}
	}
}
