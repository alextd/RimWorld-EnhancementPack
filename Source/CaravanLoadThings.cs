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
			if (!Settings.Get().caravanSaveManifest) return;

			caravan = true;
			savedManifest = new List<ThingCountUNLIMITED>();
			//bool matchesSelection = true;	//Ideally it wouldn't save if it matches selection but that's hard to figure out, can just hit reset button.

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
				if (i.Calls(CalculateAndRecacheTransferablesInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//Dialog
					yield return new CodeInstruction(OpCodes.Ldarg_0);//Dialog
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Dialog_FormCaravan), "map"));//Dialog.map
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LoadManifest), nameof(AddThings)));//AddManifest(Dialog, Dialog.map)
				}
			}
		}

		public static void AddThings(Dialog_FormCaravan dialog, Map map)
		{
			//Add manifest
			if (Settings.Get().caravanSaveManifest && SaveManifest.caravan &&
				map == SaveManifest.savedMap && SaveManifest.savedMap != null)
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
						Log.Message($"Adding Selected {thing}:{thing.stackCount}");
						TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thing, dialog.transferables, TransferAsOneMode.PodsOrCaravanPacking);
						transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thing.stackCount));
					}
			}
		}
	}


	//Now for transport pods.

	//Should be Dialog_LoadTransporters but no override of PostClose
	//[HarmonyPatch(typeof(Window), "PostClose")]
	//Okay this patch is how it should be
	//Seems on mac/linux, patching an empty virtual method causes a crash
	//So let's patch the one place this method is actually called
	//public bool TryRemove(Window window, bool doCloseSound = true)
	[HarmonyPatch(typeof(WindowStack), nameof(WindowStack.TryRemove), new Type[] { typeof(Window), typeof(bool)})]
	public static class PodsSaveManifest
	{
		public static FieldInfo transferablesInfo = AccessTools.Field(typeof(Dialog_LoadTransporters), "transferables");
		public static List<TransferableOneWay> Transferables(this Dialog_LoadTransporters dialog) =>
			(List<TransferableOneWay>)transferablesInfo.GetValue(dialog);

		//This should also be 'Map ___map' but THE PATCH IS NOT ACTUALLY Dialog_LoadTransporters NOW IS IT?
		public static FieldInfo mapInfo = AccessTools.Field(typeof(Dialog_LoadTransporters), "map");
		public static Map Map(this Dialog_LoadTransporters dialog) =>
			(Map)mapInfo.GetValue(dialog);

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo PostCloseInfo = AccessTools.Method(typeof(Window), nameof(Window.PostClose));
			foreach(CodeInstruction i in instructions)
			{
				if (i.Calls(PostCloseInfo))
				{
					yield return new CodeInstruction(OpCodes.Dup);//Window window2 = window
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PodsSaveManifest), nameof(PostClosePrefix)));//PostClosePrefix(window2)
					//followed by PostClose(window)
				}

				yield return i;
			}
		}
		
		public static void PostClosePrefix(Window __instance)
		{
			if (__instance is Dialog_LoadTransporters dialog)
			{ 
				if (!Settings.Get().caravanSaveManifest) return;

				SaveManifest.caravan = false;
				SaveManifest.savedManifest = new List<ThingCountUNLIMITED>();

				foreach (TransferableOneWay tr in dialog.Transferables())
				{
					if (tr.CountToTransfer > 0)
					{
						Log.Message($"Saving {tr.AnyThing}:{tr.CountToTransfer}");
						SaveManifest.savedManifest.Add(new ThingCountUNLIMITED(tr.AnyThing, tr.CountToTransfer));
					}
				}

				if (SaveManifest.savedManifest.Count == 0)
				{
					SaveManifest.savedManifest = null;
					SaveManifest.savedMap = null;
				}
				else
					SaveManifest.savedMap = dialog.Map();
			}
		}
	}


	[HarmonyPatch(typeof(Dialog_LoadTransporters), "PostOpen")]
	static class PodsLoadManifest
	{
		public static void Postfix(Dialog_LoadTransporters __instance, Map ___map)
		{
			//Add manifest
			if (Settings.Get().caravanSaveManifest && !SaveManifest.caravan &&
				___map == SaveManifest.savedMap && SaveManifest.savedMap != null)
			{
				foreach (ThingCountUNLIMITED thingCount in SaveManifest.savedManifest)
				{
					Log.Message($"Loading {thingCount.thing}:{thingCount.count}");
					TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thingCount.thing, __instance.Transferables(), TransferAsOneMode.PodsOrCaravanPacking);
					transferableOneWay?.AdjustTo(transferableOneWay.ClampAmount(transferableOneWay.CountToTransfer + thingCount.count));
				}
			}
			//No selection like caravans - you're already selecting pods!
		}
	}
}