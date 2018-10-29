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
	class LightingOverlay : BaseOverlay
	{
		public static Dictionary<Map, LightingOverlay> lightingOverlays = new Dictionary<Map, LightingOverlay>();
		public float skyGlow;

		public LightingOverlay(Map m) : base(m) { }

		public override bool GetCellBool(int index)
		{
			return (map.roofGrid.GetCellBool(index) || LightingAt(map, index) > skyGlow)
				&& !map.fogGrid.IsFogged(index);
		}

		public override Color GetCellExtraColor(int index)
		{
			return Color.Lerp(Color.red, Color.green, LightingAt(map, index));
		}

		public static float LightingAt(Map map, int index)
		{
			return map.glowGrid.GameGlowAt(map.cellIndices.IndexToCell(index));
		}

		public override bool ShouldDraw() => PlaySettings_Patch_Lighting.showLightingOverlay;

		public void SetDirtySky(float newSky)
		{
			if(skyGlow != newSky)
			{
				skyGlow = newSky;
				SetDirty();
			}
		}
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch_Lighting
	{
		public static void Postfix()
		{
			if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			if (!LightingOverlay.lightingOverlays.TryGetValue(Find.CurrentMap, out LightingOverlay lightingOverlay))
			{
				lightingOverlay = new LightingOverlay(Find.CurrentMap);
				LightingOverlay.lightingOverlays[Find.CurrentMap] = lightingOverlay;
			}
			lightingOverlay.Draw();
		}
	}

	[HarmonyPatch(typeof(GlowGrid), "MarkGlowGridDirty")]
	static class GlowGridDirty_Patch
	{
		public static void Postfix(GlowGrid __instance, Map ___map)
		{
			Map map = ___map;

			if (!LightingOverlay.lightingOverlays.TryGetValue(map, out LightingOverlay lightingOverlay))
			{
				lightingOverlay = new LightingOverlay(map);
				LightingOverlay.lightingOverlays[map] = lightingOverlay;
			}
			lightingOverlay.SetDirty();
		}
	}

	[HarmonyPatch(typeof(SkyManager), "UpdateOverlays")]
	static class SkyManagerDirty_Patch
	{
		//private void UpdateOverlays(SkyTarget curSky)
		public static void Postfix(SkyManager __instance, Map ___map)
		{
			Map map = ___map;

			if (!LightingOverlay.lightingOverlays.TryGetValue(map, out LightingOverlay lightingOverlay))
			{
				lightingOverlay = new LightingOverlay(map);
				LightingOverlay.lightingOverlays[map] = lightingOverlay;
			}
			lightingOverlay.SetDirtySky(__instance.CurSkyGlow);
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
			if (!Settings.Get().showOverlayLighting) return;
			if (worldView) return;

			row.ToggleableIcon(ref showLightingOverlay, icon, "TD.ToggleLighting".Translate());
		}
	}

}
