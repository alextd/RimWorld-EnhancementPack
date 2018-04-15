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
	class LightingOverlay : ICellBoolGiver
	{
		public static Dictionary<Map, LightingOverlay> lightingOverlays = new Dictionary<Map, LightingOverlay>();

		private CellBoolDrawer drawer;
		//private bool[] data;
		private Map map;

		public LightingOverlay(Map m)
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
			float f = LightingAt(map, index);
			return f != 1
				&& !map.fogGrid.IsFogged(index);
		}

		public Color GetCellExtraColor(int index)
		{
			float l = LightingAt(map, index);
			return Color.Lerp(Color.black, Color.white, l);
		}

		public static float LightingAt(Map map, int index)
		{
			return map.glowGrid.GameGlowAt(map.cellIndices.IndexToCell(index));
		}

		public void Draw()
		{
			if (PlaySettings_Patch_Lighting.showLightingOverlay)
				drawer.MarkForDraw();
			drawer.CellBoolDrawerUpdate();
		}

		public void SetDirty()
		{
			drawer.SetDirty();
		}
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch_Lighting
	{
		public static void Postfix()
		{
			if (Find.VisibleMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			if (!LightingOverlay.lightingOverlays.TryGetValue(Find.VisibleMap, out LightingOverlay lightingOverlay))
			{
				lightingOverlay = new LightingOverlay(Find.VisibleMap);
				LightingOverlay.lightingOverlays[Find.VisibleMap] = lightingOverlay;
			}
			lightingOverlay.Draw();
		}
	}

	[HarmonyPatch(typeof(GlowGrid), "MarkGlowGridDirty")]
	static class GlowGridDirty_Patch
	{
		public static void Postfix(GlowGrid __instance)
		{
			FieldInfo mapField = AccessTools.Field(typeof(GlowGrid), "map");
			Map map = (Map)mapField.GetValue(__instance);

			if (!LightingOverlay.lightingOverlays.TryGetValue(map, out LightingOverlay lightingOverlay))
			{
				lightingOverlay = new LightingOverlay(map);
				LightingOverlay.lightingOverlays[map] = lightingOverlay;
			}
			lightingOverlay.SetDirty();
		}
	}

	[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
	[StaticConstructorOnStartup]
	public static class PlaySettings_Patch_Lighting
	{
		public static bool showLightingOverlay;
		private static Texture2D icon = ContentFinder<Texture2D>.Get("LampSun", true);// or WallBricks_MenuIcon;

		[HarmonyPostfix]
		public static void AddButton(WidgetRow row, bool worldView)
		{
			if (worldView)
				return;
			row.ToggleableIcon(ref showLightingOverlay, icon, "TD.ToggleLighting".Translate());
		}
	}

}
