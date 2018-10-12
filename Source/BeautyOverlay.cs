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
	class BeautyOverlay : ICellBoolGiver
	{
		public static Dictionary<Map, BeautyOverlay> beautyOverlays = new Dictionary<Map, BeautyOverlay>();

		private CellBoolDrawer drawer;
		//private bool[] data;
		private Map map;

		public BeautyOverlay(Map m)
		{
			map = m;
			drawer = new CellBoolDrawer((ICellBoolGiver)this, map.Size.x, map.Size.z, 0.67f);
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
			float b = BeautyAt(map, index);
			return b != 0
				&& !map.fogGrid.IsFogged(index);
		}

		public Color GetCellExtraColor(int index)
		{
			float amount = BeautyAt(map, index);

			bool good = amount > 0;
			amount = amount > 0 ? amount/50 : -amount/10;

			Color baseColor = good ? Color.green : Color.red;
			baseColor.a = 0;

			return good && amount > 1 ? Color.Lerp(Color.green, Color.white, amount - 1)
				: Color.Lerp(baseColor, good ? Color.green : Color.red, amount);
		}

		public static float BeautyAt(Map map, int index)
		{
			return BeautyUtility.CellBeauty(map.cellIndices.IndexToCell(index), map);
		}

		public void Draw()
		{
			if (PlaySettings_Patch_Beauty.showBeautyOverlay)
				drawer.MarkForDraw();
			drawer.CellBoolDrawerUpdate();
		}

		public void SetDirty()
		{
			drawer.SetDirty();
		}
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch_Beauty
	{
		public static void Postfix()
		{
			if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			if (!BeautyOverlay.beautyOverlays.TryGetValue(Find.CurrentMap, out BeautyOverlay beautyOverlay))
			{
				beautyOverlay = new BeautyOverlay(Find.CurrentMap);
				BeautyOverlay.beautyOverlays[Find.CurrentMap] = beautyOverlay;
			}
			beautyOverlay.Draw();
		}
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class TerrainChangedSetDirty
	{
		public static void Postfix(Map ___map)
		{
			Map map = ___map;

			if (!BeautyOverlay.beautyOverlays.TryGetValue(map, out BeautyOverlay beautyOverlay))
			{
				beautyOverlay = new BeautyOverlay(map);
				BeautyOverlay.beautyOverlays[map] = beautyOverlay;
			}
			beautyOverlay.SetDirty();
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Register")]
	public static class ThingDirtierRegister
	{
		public static void Postfix(Map ___map)
		{
			TerrainChangedSetDirty.Postfix(___map);
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class ThingDirtierDeregister
	{
		public static void Postfix(Map ___map)
		{
			TerrainChangedSetDirty.Postfix(___map);
		}
	}


	[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
	[StaticConstructorOnStartup]
	public static class PlaySettings_Patch_Beauty
	{
		public static bool showBeautyOverlay;
		private static Texture2D icon = ContentFinder<Texture2D>.Get("Heart", true);

		[HarmonyPostfix]
		public static void AddButton(WidgetRow row, bool worldView)
		{
			if (!Settings.Get().showOverlayBeauty) return;
			if (worldView) return;

			row.ToggleableIcon(ref showBeautyOverlay, icon, "TD.ToggleBeauty".Translate());
		}
	}

}
