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
	public class ThingCountUNLIMITED  //ThingCount verifies and sets count to stackCount
	{
		public Thing thing;
		public int count;
		public ThingCountUNLIMITED(Thing t, int c)
		{
			thing = t;
			count = c;
		}
	}

	[HarmonyPatch(typeof(Dialog_FormCaravan), "PostClose")]
	static class SaveManifest
	{
		public static List<ThingCountUNLIMITED> savedManifest;//TODO clear items when destroyed I guess
		public static Map savedMap;
		//private void CalculateAndRecacheTransferables()
		public static void Prefix(Dialog_FormCaravan __instance, Map ___map)
		{
			savedManifest = new List<ThingCountUNLIMITED>();
			//bool matchesSelection = true;

			foreach (TransferableOneWay tr in __instance.transferables)
			{
				if (tr.CountToTransfer > 0)
				{
					Log.Message($"Saving {tr.AnyThing}:{tr.CountToTransfer}");
					savedManifest.Add(new ThingCountUNLIMITED(tr.AnyThing, tr.CountToTransfer));
				}
			}

			if (savedManifest.Count == 0)
			{
				savedManifest = null;
				savedMap = null;
			}
			else
				savedMap = ___map;
		}
	}

	[HarmonyPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap))]
	public static class MapRemover
	{
		//public void DeinitAndRemoveMap(Map map)
		public static void Postfix(List<Map> ___maps, Map map)
		{
			if (map == null || !___maps.Contains(map))
				return;

			if (SaveManifest.savedMap == map)
			{
				SaveManifest.savedMap = null;
				SaveManifest.savedManifest = null;
			}
		}
	}


	[HarmonyPatch(typeof(Dialog_FormCaravan), "PostOpen")]
	static class LoadManifest
	{
		//This would be postfix with !thisWindowInstanceEverOpened
		//thisWindowInstanceEverOpened is set true in method, 
		//need to transpile after the call to CalculateAndRecacheTransferables
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo CalculateAndRecacheTransferablesInfo = AccessTools.Method(typeof(Dialog_FormCaravan), "CalculateAndRecacheTransferables");

			foreach (CodeInstruction i in instructions)
			{
				yield return i;
				if (i.opcode == OpCodes.Call && i.operand == CalculateAndRecacheTransferablesInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//Dialog
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Dialog_FormCaravan), "map"));//Dialog.map
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LoadManifest), nameof(AddThings)));//AddManifest(Dialog, Dialog.map)
				}
			}
		}

		public static void AddThings(Dialog_FormCaravan dialog, Map map)
		{
			//Add manifest
			if (map == SaveManifest.savedMap && SaveManifest.savedMap != null)
			{
				foreach (ThingCountUNLIMITED thingCount in SaveManifest.savedManifest)
				{
					Log.Message($"Loading {thingCount.thing}:{thingCount.count}");
					TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thingCount.thing, dialog.transferables, TransferAsOneMode.PodsOrCaravanPacking);
					transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thingCount.count));
				}
			}
			//Add selection
			else if (Settings.Get().caravanLoadSelection)
			{
				foreach (object obj in Find.Selector.SelectedObjectsListForReading.Where(o => o is Thing))
					if (obj is Thing thing)
					{
						Log.Message($"adding {thing}:{thing.stackCount}");
						TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thing, dialog.transferables, TransferAsOneMode.PodsOrCaravanPacking);
						transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thing.stackCount));
					}
			}
		}
	}
}