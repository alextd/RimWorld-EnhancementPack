using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
	class RebuildTransportPod
	{
		//A lot like CheckAutoRebuildOnDestroyed
		public static void CheckAutoRebuildOnLaunch(Thing thing, Map map, BuildableDef buildingDef)
		{
			if (!Settings.Get().autoRebuildTransportPod) return;

			if (Find.PlaySettings.autoRebuild && thing.Faction == Faction.OfPlayer && buildingDef.blueprintDef != null && buildingDef.IsResearchFinished && map.areaManager.Home[thing.Position] && GenConstruct.CanPlaceBlueprintAt(buildingDef, thing.Position, thing.Rotation, map, false, null).Accepted)
			{
				GenConstruct.PlaceBlueprintForBuild(buildingDef, thing.Position, map, thing.Rotation, Faction.OfPlayer, thing.Stuff);
			}
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo DestroyInfo = AccessTools.Method(typeof(Thing), nameof(Thing.Destroy));
			MethodInfo DestroyReplaceInfo = AccessTools.Method(typeof(RebuildTransportPod), nameof(DestroyReplace));

			return Transpilers.MethodReplacer(instructions, DestroyInfo, DestroyReplaceInfo);
		}

		public static void DestroyReplace(Thing thing, DestroyMode mode)
		{
			Map map = thing.Map;//Save map before it's destroyed
			thing.Destroy(mode);
			CheckAutoRebuildOnLaunch(thing, map, thing.def);
		}
	}
}
