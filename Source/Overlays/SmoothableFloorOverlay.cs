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
	class SmoothableOverlay : BaseOverlay
	{
		public static Dictionary<Map, SmoothableOverlay> smoothableOverlays = new Dictionary<Map, SmoothableOverlay>();

		public SmoothableOverlay(Map m) : base(m) { }

		public override bool GetCellBool(int index)
		{
			if (map.fogGrid.IsFogged(index))
				return false;
			
			return map.terrainGrid.TerrainAt(index).affordances.Contains(TerrainAffordanceDefOf.SmoothableStone);
		}
		public override Color GetCellExtraColor(int index) => Color.green;

		public override bool ShouldAutoDraw() => Settings.Get().autoOverlaySmoothable;
		public override Type AutoDesignator() => typeof(Designator_SmoothSurface);
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch_Smoothable
	{
		public static void Postfix()
		{
			if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			if (!SmoothableOverlay.smoothableOverlays.TryGetValue(Find.CurrentMap, out SmoothableOverlay smoothableOverlay))
			{
				smoothableOverlay = new SmoothableOverlay(Find.CurrentMap);
				SmoothableOverlay.smoothableOverlays[Find.CurrentMap] = smoothableOverlay;
			}
			smoothableOverlay.Draw();
		}
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch_Smoothable
	{
		public static void Postfix(TerrainGrid __instance, Map ___map)
		{
			Map map = ___map;

			if (!SmoothableOverlay.smoothableOverlays.TryGetValue(map, out SmoothableOverlay smoothableOverlay))
			{
				smoothableOverlay = new SmoothableOverlay(map);
				SmoothableOverlay.smoothableOverlays[map] = smoothableOverlay;
			}
			smoothableOverlay.SetDirty();
		}
	}
}
