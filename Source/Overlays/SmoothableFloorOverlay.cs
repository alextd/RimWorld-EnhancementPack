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

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	class SmoothableOverlay : BaseOverlay
	{
		public SmoothableOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			Map map = Find.CurrentMap;
			if (map.edificeGrid[index] is Thing wall)
				return wall.def.IsSmoothable;
			else
				return map.terrainGrid.TerrainAt(index).affordances.Contains(TerrainAffordanceDefOf.SmoothableStone);
		}
		public override Color GetCellExtraColor(int index) => Color.green;

		public override bool ShouldAutoDraw() => Mod.settings.autoOverlaySmoothable;
		public override Type AutoDesignator() => typeof(Designator_SmoothSurface);
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch_Smoothable
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(SmoothableOverlay));
		}
	}
}
