using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace TD_Enhancement_Pack.Overlays
{
	[StaticConstructorOnStartup]
	class BuildableOverlay : BaseOverlay
	{
		public static readonly Color noneColor = new Color(1, 0, 0);
		public static readonly Color lightColor = new Color(.8f, .4f, 0);
		public static readonly Color mediumColor = new Color(.8f, .8f, 0);

		//Extras class to handle any specific overlay for specific defs
		public static List<BuildableExtras> extras = new List<BuildableExtras>();
		public static BuildableExtras ActiveExtra() => extras.FirstOrDefault(ex => ex.active);

		static BuildableOverlay()
		{
			extras.Clear();
			foreach (Type current in typeof(BuildableExtras).AllLeafSubclasses())
				extras.Add((BuildableExtras)Activator.CreateInstance(current));
		}

		public BuildableOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			foreach (BuildableExtras extra in extras.Where(ex => ex.active))
				return extra.ShowCell(index);

			return !Find.CurrentMap.terrainGrid.TerrainAt(index).affordances.Contains(TerrainAffordanceDefOf.Heavy);
		}

		public override Color GetCellExtraColor(int index)
		{
			foreach (BuildableExtras extra in extras.Where(ex => ex.active))
				return extra.GetCellExtraColor(index);

			var affordances = Find.CurrentMap.terrainGrid.TerrainAt(index).affordances;
			return affordances.Contains(TerrainAffordanceDefOf.Medium) ? mediumColor :
				affordances.Contains(TerrainAffordanceDefOf.Light) ? lightColor
				: noneColor;
		}

		public override void Update()
		{
			if (Find.DesignatorManager.SelectedDesignator is Designator_Place des
				&& des.PlacingDef is ThingDef def)
			{
				foreach (BuildableExtras extra in extras)
					if (extra.MakeActive(def))
						SetDirty();
			}
			else
				foreach (BuildableExtras extra in extras)
					if (extra.active)
					{
						extra.active = false;
						SetDirty();
					}

			base.Update();
		}

		public override bool ShouldAutoDraw() => Mod.settings.autoOverlayBuildable;
		public override Type AutoDesignator() => typeof(Designator_Place);

		public static Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/StoneBlocks", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Mod.settings.showOverlayBuildable;
		public override string IconTip() => "TD.ToggleBuildable".Translate();
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(BuildableOverlay));
		}
	}

	//Overide Designator_build things based on def
	public abstract class BuildableExtras
	{
		public bool active;

		public abstract bool ShowCell(int index);
		public virtual Color GetCellExtraColor(int index) => Color.green;
		public abstract bool Matches(ThingDef def);
		public bool MakeActive(ThingDef def)
		{
			bool nowActive = Matches(def);
			if (nowActive != active)
			{
				active = nowActive;
				return true;
			}
			return false;
		}
	}

	//Don't do moisture pump: coverage overlay handles it.
	//This is an annoying place to decide this
	public class MoisturePumpExtra : BuildableExtras
	{
		public override bool ShowCell(int index) => false;

		public override bool Matches(ThingDef def) => def == MoreThingDefOf.MoisturePump;
	}

	//Geothermal generator can only be placed on certain tiles, highlight them instead.
	public class GeothermalExtra : BuildableExtras
	{
		public override bool ShowCell(int index) =>
				Find.CurrentMap.thingGrid.ThingsListAtFast(index).Any(t => t.def == ThingDefOf.SteamGeyser);

		public override bool Matches(ThingDef def) => def == ThingDefOf.GeothermalGenerator;
	}
	
	//Most things needs light/medium/heavy, so BuildableOverlay handles those normally: clear/yellow/red
	//If the thing to build needs something else, use special affordance overaly with clear/red
	public class SpecialAffordanceExtra : BuildableExtras
	{
		public override bool ShowCell(int index) =>
			!Find.CurrentMap.terrainGrid.TerrainAt(index).affordances.Contains(curAffordance);

		public override Color GetCellExtraColor(int index) => BuildableOverlay.noneColor;

		public TerrainAffordanceDef curAffordance;
		public override bool Matches(ThingDef def)
		{
			curAffordance = def.terrainAffordanceNeeded;
			return curAffordance != null
				&& curAffordance != TerrainAffordanceDefOf.Light
				&& curAffordance != TerrainAffordanceDefOf.Medium
				&& curAffordance != TerrainAffordanceDefOf.Heavy;
		}
	}
}