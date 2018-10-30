using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	class TreeGrowthOverlay : BaseOverlay
	{
		public TreeGrowthOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsListAtFast(index))
				if (thing.def.plant?.harvestTag == "Wood")
					return true;
			return false;
		}
		public override Color GetCellExtraColor(int index)
		{
			Plant tree = Find.CurrentMap.thingGrid.ThingsListAtFast(index).FirstOrDefault(t => t.def.plant?.harvestTag == "Wood") as Plant;
			if (tree == null) return Color.magenta;//shouldn't happen

			return tree.LifeStage == PlantLifeStage.Mature ? Color.white :
				tree.HarvestableNow ? Color.green :
				tree.HarvestableSoon ? Color.yellow :
				Color.red;
		}


		public override bool ShouldAutoDraw() => Settings.Get().autoOverlayTreeGrowth;
		public override Type AutoDesignator() => typeof(Designator_PlantsHarvestWood);
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class ThingDirtierDeregister_TreeGrowth
	{
		public static void Postfix(Thing t, Map ___map)
		{
			if (___map == Find.CurrentMap)
				if (t is Plant)
					BaseOverlay.SetDirty(typeof(TreeGrowthOverlay));
		}
	}

	[HarmonyPatch(typeof(Plant), "PlantCollected")]
	public static class PlantCollected
	{
		//public virtual void PlantCollected()
		public static void Postfix(Plant __instance)
		{
			if (__instance.Map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(TreeGrowthOverlay));
		}
	}

	[HarmonyPatch(typeof(TickList), "Tick")]
	public static class TickGrow
	{
		public static void Postfix(TickerType ___tickType)
		{
			if (___tickType == TickerType.Long)
				BaseOverlay.SetDirty(typeof(TreeGrowthOverlay));
		}
	}
}
