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
	class BuildableOverlay : ICellBoolGiver
	{
		public static Dictionary<Map, BuildableOverlay> buildableOverlays = new Dictionary<Map, BuildableOverlay>();

		public static readonly Color noneColor = new Color(1, 0, 0);
		public static readonly Color lightColor = new Color(.8f, .8f, 0);

		private CellBoolDrawer drawer;
		//private bool[] data;
		private Map map;

		public BuildableOverlay(Map m)
		{
			map = m;
			drawer = new CellBoolDrawer((ICellBoolGiver)this, map.Size.x, map.Size.z);
			//data = new bool[map.cellIndices.NumGridCells];
		}

		public Color Color
		{
			get
			{
				return new Color(1f, 1f, 1f);
			}
		}

		public bool GetCellBool(int index)
		{
			return !map.terrainGrid.TerrainAt(index).affordances.Contains(TerrainAffordance.Heavy) &&
				!map.fogGrid.IsFogged(index);
		}

		public Color GetCellExtraColor(int index)
		{
			return map.terrainGrid.TerrainAt(index).affordances.Contains(TerrainAffordance.Light)
				? lightColor : noneColor ;
		}

		public void Draw()
		{
			if (PlaySettings_Patch.showBuildableOverlay)
				drawer.MarkForDraw();
			drawer.CellBoolDrawerUpdate();
		}

		public void SetDirty()
		{
			drawer.SetDirty();
		}
	}
	
	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch
	{
		public static void Postfix()
		{
			if (Find.VisibleMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			if (!BuildableOverlay.buildableOverlays.TryGetValue(Find.VisibleMap, out BuildableOverlay buildableOverlay))
			{
				buildableOverlay = new BuildableOverlay(Find.VisibleMap);
				BuildableOverlay.buildableOverlays[Find.VisibleMap] = buildableOverlay;
			}
			buildableOverlay.Draw();
		}
	}
	
	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch
	{
		public static void Postfix(TerrainGrid __instance)
		{
			FieldInfo mapField = AccessTools.Field(typeof(TerrainGrid), "map");
			Map map = (Map)mapField.GetValue(__instance);

			if (!BuildableOverlay.buildableOverlays.TryGetValue(map, out BuildableOverlay buildableOverlay))
			{
				buildableOverlay = new BuildableOverlay(map);
				BuildableOverlay.buildableOverlays[map] = buildableOverlay;
			}
			buildableOverlay.SetDirty();
		}
	}

 [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
	[StaticConstructorOnStartup]
	public static class PlaySettings_Patch
	{
		public static bool showBuildableOverlay;
		private static Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/StoneBlocks", true);// or WallBricks_MenuIcon;

		[HarmonyPostfix]
		public static void AddButton(WidgetRow row, bool worldView)
		{
			if (worldView)
				return;
			row.ToggleableIcon(ref showBuildableOverlay, icon, "ToggleBuildable".Translate());
		}
	}

}
