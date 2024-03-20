using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

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
		public static bool caravan;//or pods

		public static void Prefix(Dialog_FormCaravan __instance, Map ___map)
		{
			Save(true, __instance.transferables, ___map);
		}

		public static void Save(bool forCaravan, List<TransferableOneWay> transferables, Map map)
		{
			if (!Mod.settings.caravanSaveManifest) return;

			caravan = forCaravan;
			savedManifest = new List<ThingCountUNLIMITED>();
			//bool matchesSelection = true;	//Ideally it wouldn't save if it matches selection but that's hard to figure out, can just hit reset button.

			foreach (TransferableOneWay tr in transferables)
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
				savedMap = map;
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
				if (i.Calls(CalculateAndRecacheTransferablesInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//Dialog
					yield return new CodeInstruction(OpCodes.Ldarg_0);//Dialog
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Dialog_FormCaravan), "map"));//Dialog.map
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LoadManifest), nameof(AddThings)));//AddManifest(Dialog, Dialog.map)
				}
			}
		}

		public static void Load(List<TransferableOneWay> transferables)
		{
			foreach (ThingCountUNLIMITED thingCount in SaveManifest.savedManifest)
			{
				Log.Message($"Loading {thingCount.thing}:{thingCount.count}");
				TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thingCount.thing, transferables, TransferAsOneMode.PodsOrCaravanPacking);
				transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thingCount.count));
			}
		}

		public static void AddThings(Dialog_FormCaravan dialog, Map map)
		{
			//Add manifest
			if (Mod.settings.caravanSaveManifest && SaveManifest.caravan &&
				map == SaveManifest.savedMap && SaveManifest.savedMap != null)
			{
				Load(dialog.transferables);
			}
			//Add selection
			else if (Mod.settings.caravanLoadSelection)
			{
				foreach (object obj in Find.Selector.SelectedObjectsListForReading.Where(o => o is Thing))
					if (obj is Thing thing)
					{
						Log.Message($"Adding Selected {thing}:{thing.stackCount}");
						TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thing, dialog.transferables, TransferAsOneMode.PodsOrCaravanPacking);
						transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thing.stackCount));
					}
			}
		}
	}


	//Now for transport pods.

	[HarmonyPatch(typeof(Window), "PostClose")]
	public static class PodsSaveManifest
	{
		public static AccessTools.FieldRef<Dialog_LoadTransporters, List<TransferableOneWay>> Transferables =
			AccessTools.FieldRefAccess<Dialog_LoadTransporters, List<TransferableOneWay>>("transferables");

		public static AccessTools.FieldRef<Dialog_LoadTransporters, Map> Map =
			AccessTools.FieldRefAccess<Dialog_LoadTransporters, Map>("map");
		
		public static void Prefix(Window __instance)
		{
			if (__instance is Dialog_LoadTransporters dialog)
			{
				SaveManifest.Save(false, Transferables(dialog), Map(dialog));
			}
		}
	}


	[HarmonyPatch(typeof(Dialog_LoadTransporters), "PostOpen")]
	static class PodsLoadManifest
	{
		public static void Postfix(Dialog_LoadTransporters __instance, Map ___map)
		{
			//Add manifest
			if (Mod.settings.caravanSaveManifest && !SaveManifest.caravan &&
				___map == SaveManifest.savedMap && SaveManifest.savedMap != null)
			{
				LoadManifest.Load(PodsSaveManifest.Transferables(__instance));
			}
			//No selection like caravans - you're already selecting pods!
		}
	}
}