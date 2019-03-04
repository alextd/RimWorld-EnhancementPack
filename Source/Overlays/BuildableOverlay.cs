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
	[DefOf]
	public static class MoreThingDefOf
	{
		public static ThingDef MoisturePump;
	}

	[StaticConstructorOnStartup]
	class BuildableOverlay : BaseOverlay
	{
		public static readonly Color noneColor = new Color(1, 0, 0);
		public static readonly Color lightColor = new Color(.8f, .4f, 0);
		public static readonly Color mediumColor = new Color(.8f, .8f, 0);

		public static List<BuildableExtras> extras = new List<BuildableExtras>(); 

		static BuildableOverlay()
		{
			extras.Clear();
			foreach (Type current in typeof(BuildableExtras).AllLeafSubclasses())
				extras.Add((BuildableExtras)Activator.CreateInstance(current));
		}


		public BuildableOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			foreach (BuildableExtras extra in extras)
				if (extra.active)
					return extra.ShowCell(index);

			if (placingMoisturePump)
			{
				return Find.CurrentMap.terrainGrid.TerrainAt(index)?.driesTo != null ||
					Find.CurrentMap.terrainGrid.UnderTerrainAt(index)?.driesTo != null;
			}
			return !Find.CurrentMap.terrainGrid.TerrainAt(index).affordances.Contains(TerrainAffordanceDefOf.Heavy);
		}

		public override Color GetCellExtraColor(int index)
		{
			foreach (BuildableExtras extra in extras)
				if (extra.active)
					return extra.GetCellExtraColor(index);

			if (placingMoisturePump)
			{
				return moisturePumpCells.Contains(Find.CurrentMap.cellIndices.IndexToCell(index))
					? Color.blue * 0.5f : Color.green;
			}

			var affordances = Find.CurrentMap.terrainGrid.TerrainAt(index).affordances;
			return affordances.Contains(TerrainAffordanceDefOf.Medium) ? mediumColor :
				affordances.Contains(TerrainAffordanceDefOf.Light) ? lightColor
				: noneColor;
		}

		//Buildings that cover an area with an aura shows total coverage
		private static HashSet<IntVec3> moisturePumpCells = new HashSet<IntVec3>();
		public bool placingMoisturePump;
		public bool PlacingMoisturePump()
		{
			if (Find.DesignatorManager.SelectedDesignator is Designator_Build des)
			{
				return des.PlacingDef == MoreThingDefOf.MoisturePump;
			}

			return false;
		}

		public override void Update()
		{
			if (Find.DesignatorManager.SelectedDesignator is Designator_Build des)
			{
				BuildableDef def = des.PlacingDef;
				foreach (BuildableExtras extra in extras)
					if (extra.MakeActive(def))
						SetDirty();
			}

			bool newPump = PlacingMoisturePump();
			if (newPump != placingMoisturePump)
			{
				Log.Message($"newPump is {newPump}");
				placingMoisturePump = newPump;
				SetDirty();
			}
			if (placingMoisturePump && dirty)
			{
				HashSet<IntVec3> centers = new HashSet<IntVec3>(Find.CurrentMap.listerThings.ThingsOfDef(MoreThingDefOf.MoisturePump).Select(t => t.Position));

				centers.AddRange(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
					.Where(bp => GenConstruct.BuiltDefOf(bp.def) == MoreThingDefOf.MoisturePump).Select(t => t.Position).ToList());

				centers.AddRange(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
					.Where(frame => GenConstruct.BuiltDefOf(frame.def) == MoreThingDefOf.MoisturePump).Select(t => t.Position).ToList());

				moisturePumpCells.Clear();

				float radius = MoreThingDefOf.MoisturePump.specialDisplayRadius;
				foreach (IntVec3 center in centers)
				{
					int num = GenRadial.NumCellsInRadius(radius);
					for (int i = 0; i < num; i++)
						moisturePumpCells.Add(center + GenRadial.RadialPattern[i]);
				}
			}

			base.Update();
		}

		public override void PostDraw()
		{
			if (placingMoisturePump)
			{
				GenDraw.DrawFieldEdges(moisturePumpCells.ToList(), Color.blue);
			}
		}

		public override bool ShouldAutoDraw() => Settings.Get().autoOverlayBuildable;
		public override Type AutoDesignator() => typeof(Designator_Build);

		public static Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/StoneBlocks", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Settings.Get().showOverlayBuildable;
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

	[HarmonyPatch(typeof(ThingGrid), "Register")]
	public static class ThingDirtierRegister_Pump
	{
		public static void Postfix(Thing t, Map ___map)
		{
			if (___map == Find.CurrentMap)
				if (GenConstruct.BuiltDefOf(t.def) == MoreThingDefOf.MoisturePump)
					BaseOverlay.SetDirty(typeof(BuildableOverlay));
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class ThingDirtierDeregister_Pump
	{
		public static void Postfix(Thing t, Map ___map)
		{
			if (___map == Find.CurrentMap)
				if (GenConstruct.BuiltDefOf(t.def) == MoreThingDefOf.MoisturePump)
					BaseOverlay.SetDirty(typeof(BuildableOverlay));
		}
	}

	//Overide Designator_build things based on def
	public abstract class BuildableExtras
	{
		public bool active;

		public abstract bool ShowCell(int index);
		public abstract Color GetCellExtraColor(int index);
		public abstract bool Matches(BuildableDef def);
		public bool MakeActive(BuildableDef def)
		{
			bool nowActive = Matches(def);
			if (nowActive != active)
			{
				active = nowActive;
				return true;
			}
			return false;
		}
		public virtual void PostDraw() { }
	}

	//Geothermal generator can only be placed on certain tiles, highlight them instead.
	public class GeothermalExtra : BuildableExtras
	{
		public override bool ShowCell(int index) =>
				Find.CurrentMap.thingGrid.ThingsListAtFast(index).Any(t => t.def == ThingDefOf.SteamGeyser);
		public override Color GetCellExtraColor(int index) => Color.green;
		public override bool Matches(BuildableDef def) => def == ThingDefOf.GeothermalGenerator;
	}
	
	//Most things needs light/medium/heavy, so BuildableOverlay handles those normally: clear/yellow/red
	//If the thing to build needs something else, use special affordance overaly with clear/red
	public class SpecialAffordanceExtra : BuildableExtras
	{
		public override bool ShowCell(int index) =>
			!Find.CurrentMap.terrainGrid.TerrainAt(index).affordances.Contains(curAffordance);
		public override Color GetCellExtraColor(int index) => BuildableOverlay.noneColor;

		public TerrainAffordanceDef curAffordance;
		public override bool Matches(BuildableDef def)
		{
			curAffordance = def.terrainAffordanceNeeded;
			return curAffordance != TerrainAffordanceDefOf.Light
				&& curAffordance != TerrainAffordanceDefOf.Medium
				&& curAffordance != TerrainAffordanceDefOf.Heavy;
		}

	}
}