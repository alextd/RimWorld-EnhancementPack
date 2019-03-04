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

namespace TD_Enhancement_Pack.Overlays
{
	[StaticConstructorOnStartup]
	class CoverageOverlay : BaseOverlay
	{
		//CoverageType class to handle each def
		public static List<CoverageType> types = new List<CoverageType>();
		public static CoverageType activeType;

		static CoverageOverlay()
		{
			types.Clear();
			foreach (Type current in typeof(CoverageType).AllLeafSubclasses())
				types.Add((CoverageType)Activator.CreateInstance(current));
		}

		public CoverageOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			return activeType?.ShowCell(index) ?? false;
		}

		public override Color GetCellExtraColor(int index)
		{
			return activeType?.GetCellExtraColor(index) ?? Color.white;
		}

		public override void Update()
		{
			//Find selected thing or thing to build
			ThingDef def = null;
			if (Find.DesignatorManager.SelectedDesignator is Designator_Place des)
				def = des.PlacingDef as ThingDef;
			if (def == null)
				def = Find.Selector.SingleSelectedThing.GetInnerIfMinified()?.def;
			if (def != null)
			{
				def = GenConstruct.BuiltDefOf(def) as ThingDef;
				foreach (CoverageType cov in types)
					if (cov.MakeActive(def))
					{
						SetDirty();
						if (cov.active) activeType = cov;
						else Clear();
					}

				if (dirty)  //From MakeActive or otherwise
					activeType?.Init();
			}
			else if (activeType != null)
			{
				Clear();
			}

			base.Update();
		}

		public override void Clear()
		{
			if (activeType != null)
			{
				activeType.active = false;
				activeType.Clear();
				activeType = null;
				SetDirty();
			}
		}

		public override void PostDraw()
		{
			activeType?.PostDraw();
		}

		public override bool ShouldAutoDraw() => Settings.Get().autoOverlayCoverage;
		public override Type AutoDesignator() => typeof(Designator_Place);
		public static Texture2D TexIcon = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Add", true);
		public override Texture2D Icon() => TexIcon;
		public override bool IconEnabled() => Settings.Get().showOverlayCoverage;
		public override string IconTip() => "TD.ToggleCoverage".Translate();
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Coverage_Patch
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(CoverageOverlay));//because moisture pumps change terrain
		}
	}
	//Buildings that cover an area with an aura shows total coverage
	public abstract class CoverageType
	{
		public bool active;

		public static Color coveredColor = Color.blue * 0.5f;
		protected static HashSet<IntVec3> covered = new HashSet<IntVec3>();

		public virtual bool ShowCell(int index) =>
			covered.Contains(Find.CurrentMap.cellIndices.IndexToCell(index));

		public virtual Color GetCellExtraColor(int index) => coveredColor;

		public abstract ThingDef MatchingDef();
		public virtual float Radius() => MatchingDef().specialDisplayRadius;
		public bool MakeActive(ThingDef def)
		{
			bool nowActive = def == MatchingDef();
			if (nowActive != active)
			{
				active = nowActive;
				return true;
			}
			return false;
		}

		public void Init()
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

		public void Clear()
		{
			covered.Clear();
		}

		public void PostDraw()
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
	public class TradeBeaconType : CoverageType
	{
		public override ThingDef MatchingDef() => ThingDefOf.OrbitalTradeBeacon;
		public override float Radius() => 7.9f;//Building_OrbitalTradeBeacon.TradeRadius;
	}
	public class SunLampType : CoverageType
	{
		public override ThingDef MatchingDef() => MoreThingDefOf.SunLamp;
	}
	public class FirefoamPopperType : CoverageType
	{
		public override ThingDef MatchingDef() => ThingDefOf.FirefoamPopper;
	}
	public class PsychicEmanatorType : CoverageType
	{
		public override ThingDef MatchingDef() => ThingDefOf.PsychicEmanator;
	}

	//Moisture pumps show overlay AND coverage
	public class MoisturePumpType : CoverageType
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
				if (CoverageOverlay.activeType != null &&
					GenConstruct.BuiltDefOf(t.def) == CoverageOverlay.activeType.MatchingDef())
					BaseOverlay.SetDirty(typeof(CoverageOverlay));
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