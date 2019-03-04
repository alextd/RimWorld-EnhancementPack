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
			if (Find.DesignatorManager.SelectedDesignator is Designator_Build des)
			{
				BuildableDef def = des.PlacingDef;
				foreach (BuildableExtras extra in extras)
					if (extra.MakeActive(def))
						SetDirty();

				if (dirty)  //From MakeActive or otherwise
					foreach (BuildableExtras extra in extras.Where(ex => ex.active))
						extra.Init();
			}

			base.Update();
		}

		public override void PostDraw()
		{
			foreach (BuildableExtras extra in extras.Where(ex => ex.active))
				extra.PostDraw();
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

	//Overide Designator_build things based on def
	public abstract class BuildableExtras
	{
		public bool active;

		public abstract bool ShowCell(int index);
		public virtual Color GetCellExtraColor(int index) => Color.green;
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
		public virtual void Init() { }
		public virtual void PostDraw() { }
	}

	//Geothermal generator can only be placed on certain tiles, highlight them instead.
	public class GeothermalExtra : BuildableExtras
	{
		public override bool ShowCell(int index) =>
				Find.CurrentMap.thingGrid.ThingsListAtFast(index).Any(t => t.def == ThingDefOf.SteamGeyser);

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

	//Buildings that cover an area with an aura shows total coverage
	public abstract class CoverageExtra : BuildableExtras
	{
		public static Color coveredColor = Color.blue * 0.5f;
		protected static HashSet<IntVec3> covered = new HashSet<IntVec3>();

		public override bool ShowCell(int index) =>
			covered.Contains(Find.CurrentMap.cellIndices.IndexToCell(index));

		public override Color GetCellExtraColor(int index) => coveredColor;

		public abstract ThingDef MatchingDef();
		public virtual float Radius() => MatchingDef().specialDisplayRadius;
		public override bool Matches(BuildableDef def) => def == MatchingDef();

		public override void Init()
		{
			HashSet<IntVec3> centers = new HashSet<IntVec3>(Find.CurrentMap.listerThings.ThingsOfDef(MatchingDef()).Select(t => t.Position));

			centers.AddRange(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
				.Where(bp => GenConstruct.BuiltDefOf(bp.def) == MatchingDef()).Select(t => t.Position).ToList());

			centers.AddRange(Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
				.Where(frame => GenConstruct.BuiltDefOf(frame.def) == MatchingDef()).Select(t => t.Position).ToList());

			covered.Clear();

			float radius = Radius();
			foreach (IntVec3 center in centers)
			{
				int num = GenRadial.NumCellsInRadius(radius);
				for (int i = 0; i < num; i++)
					covered.Add(center + GenRadial.RadialPattern[i]);
			}
		}

		public override void PostDraw()
		{
			GenDraw.DrawFieldEdges(covered.ToList(), Color.blue);
		}
	}

	//Things that have coverage
	//Could this list be automated? Things that have a range and that range overlap is not additive? Nah.
	[DefOf]
	public static class MoreThingDefOf
	{
		public static ThingDef MoisturePump;
		public static ThingDef SunLamp;
	}
	public class TradeBeaconExtra : CoverageExtra
	{
		public override ThingDef MatchingDef() => ThingDefOf.OrbitalTradeBeacon;
		public override float Radius() => 7.9f;//Building_OrbitalTradeBeacon.TradeRadius;
	}
	public class SunLampExtra : CoverageExtra
	{
		public override ThingDef MatchingDef() => MoreThingDefOf.SunLamp;
	}
	public class FirefoamPopperExtra : CoverageExtra
	{
		public override ThingDef MatchingDef() => ThingDefOf.FirefoamPopper;
	}

	//Moisture pumps show overlay AND coverage
	public class MoisturePumpExtra : CoverageExtra
	{
		public override bool ShowCell(int index) =>
			Find.CurrentMap.terrainGrid.TerrainAt(index)?.driesTo != null ||
			Find.CurrentMap.terrainGrid.UnderTerrainAt(index)?.driesTo != null;

		public override Color GetCellExtraColor(int index) =>
			covered.Contains(Find.CurrentMap.cellIndices.IndexToCell(index))
				? coveredColor : Color.green;

		public override ThingDef MatchingDef() => MoreThingDefOf.MoisturePump;
	}

	[HarmonyPatch(typeof(ThingGrid), "Register")]
	public static class BuildingDirtierRegister
	{
		public static void Postfix(Thing t, Map ___map)
		{
			if (___map == Find.CurrentMap)
				if (BuildableOverlay.ActiveExtra() is CoverageExtra extra &&
					GenConstruct.BuiltDefOf(t.def) == extra.MatchingDef())
					BaseOverlay.SetDirty(typeof(BuildableOverlay));
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class BuildingDirtierDeregister
	{
		public static void Postfix(Thing t, Map ___map)
		{
			BuildingDirtierRegister.Postfix(t, ___map);
		}
	}
}