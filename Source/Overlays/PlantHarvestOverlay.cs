using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	class PlantHarvestOverlay : BaseOverlay
	{
		public PlantHarvestOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsListAtFast(index))
				if (thing.def.plant?.harvestTag == "Standard")
					return true;
			return false;
		}
		public override Color GetCellExtraColor(int index)
		{
			Plant plant = Find.CurrentMap.thingGrid.ThingsListAtFast(index).FirstOrDefault(t => t.def.plant?.harvestTag == "Standard") as Plant;
			if (plant == null) return Color.magenta;//shouldn't happen

			return plant.LifeStage == PlantLifeStage.Mature ? Color.white :
				Color.Lerp(Color.red, Color.green, plant.Growth);
		}


		public override bool ShouldAutoDraw() => Mod.settings.autoOverlayPlantHarvest;
		public override Type AutoDesignator() => typeof(Designator_PlantsHarvest);

		public static Texture2D icon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Mod.settings.showOverlayPlantHarvest;
		public override string IconTip() => "TD.TogglePlantHarveset".Translate();
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class ThingDirtierDeregister_PlantHarvest
	{
		public static void Postfix(Thing t, Map ___map)
		{
			if (___map == Find.CurrentMap)
				if (t is Plant)
					BaseOverlay.SetDirty(typeof(PlantHarvestOverlay));
		}
	}

	[HarmonyPatch(typeof(Plant), "PlantCollected")]
	public static class PlantCollected_PlantHarvest
	{
		//public virtual void PlantCollected()
		public static void Postfix(Plant __instance)
		{
			if (__instance.Map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(PlantHarvestOverlay));
		}
	}

	[HarmonyPatch(typeof(TickList), "Tick")]
	public static class TickGrow_PlantHarvest
	{
		public static void Postfix(TickerType ___tickType)
		{
			if (___tickType == TickerType.Long)
				BaseOverlay.SetDirty(typeof(PlantHarvestOverlay));
		}
	}
}
