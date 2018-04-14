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
	class FertilityOverlay : ICellBoolGiver
	{
		public static Dictionary<Map, FertilityOverlay> fertilityOverlays = new Dictionary<Map, FertilityOverlay>();

		public static readonly Color noneColor = new Color(1, 0, 0);
		public static readonly Color lightColor = new Color(.8f, .8f, 0);

		private CellBoolDrawer drawer;
		//private bool[] data;
		private Map map;

		public FertilityOverlay(Map m)
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
			float f = map.terrainGrid.TerrainAt(index).fertility;
			return f != 1
				&& !map.fogGrid.IsFogged(index);
		}

		public Color GetCellExtraColor(int index)
		{
			float f = map.terrainGrid.TerrainAt(index).fertility;
			return f < 1 ? Color.Lerp(Color.red, Color.yellow, f)
				: Color.Lerp(Color.green, Color.white, f-1);
		}

		public void Draw()
		{
			if (PlaySettings_Patch_Fertility.showFertilityOverlay)
				drawer.MarkForDraw();
			drawer.CellBoolDrawerUpdate();
		}

		public void SetDirty()
		{
			drawer.SetDirty();
		}
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch_Fertility
	{
		public static void Postfix()
		{
			if (Find.VisibleMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			if (!FertilityOverlay.fertilityOverlays.TryGetValue(Find.VisibleMap, out FertilityOverlay fertilityOverlay))
			{
				fertilityOverlay = new FertilityOverlay(Find.VisibleMap);
				FertilityOverlay.fertilityOverlays[Find.VisibleMap] = fertilityOverlay;
			}
			fertilityOverlay.Draw();
		}
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch_Fertility
	{
		public static void Postfix(TerrainGrid __instance)
		{
			FieldInfo mapField = AccessTools.Field(typeof(TerrainGrid), "map");
			Map map = (Map)mapField.GetValue(__instance);

			if (!FertilityOverlay.fertilityOverlays.TryGetValue(map, out FertilityOverlay fertilityOverlay))
			{
				fertilityOverlay = new FertilityOverlay(map);
				FertilityOverlay.fertilityOverlays[map] = fertilityOverlay;
			}
			fertilityOverlay.SetDirty();
		}
	}

	[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
	[StaticConstructorOnStartup]
	public static class PlaySettings_Patch_Fertility
	{
		public static bool showFertilityOverlay;
		private static Texture2D icon = ContentFinder<Texture2D>.Get("CornPlantIcon", true);// or WallBricks_MenuIcon;

		[HarmonyPostfix]
		public static void AddButton(WidgetRow row, bool worldView)
		{
			if (worldView)
				return;
			row.ToggleableIcon(ref showFertilityOverlay, icon, "TD.ToggleFertility".Translate());
		}
	}

}
