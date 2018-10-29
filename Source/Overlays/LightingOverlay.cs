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
		public float skyGlow;

		public LightingOverlay(Map m) : base(m) { }

		public override bool ShowCell(int index)
		{
			return map.roofGrid.GetCellBool(index) || LightingAt(index) > skyGlow || GlowerColorAt(index) != null;
		}

		public override Color GetCellExtraColor(int index)
		{
			return GlowerColorAt(index) ?? Color.Lerp(Color.red, Color.green, LightingAt(index));
		}

		public float LightingAt(int index)
		{
			return map.glowGrid.GameGlowAt(map.cellIndices.IndexToCell(index));
		}

		public Color? GlowerColorAt(int index)
		{
			foreach(Thing thing in map.thingGrid.ThingsListAtFast(index))
				if(thing.TryGetComp<CompGlower>() is CompGlower compGlower)// && compGlower.ShouldBeLitNow)//ShouldBeLitNow private :/
					return Color.white;
			return null;
		}

		public void SetDirtySky(float newSky)
		{
			if(skyGlow != newSky)
			{
				skyGlow = newSky;
				SetDirty();
			}
		}

		private static Texture2D icon = ContentFinder<Texture2D>.Get("LampSun", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Settings.Get().showOverlayLighting;//from Settings
		public override string IconTip() => "TD.ToggleLighting".Translate();
	}

	[HarmonyPatch(typeof(GlowGrid), "MarkGlowGridDirty")]
	static class GlowGridDirty_Patch
	{
		public static void Postfix(GlowGrid __instance, Map ___map)
		{
			BaseOverlay.SetDirty(typeof(LightingOverlay), ___map);
		}
	}

	[HarmonyPatch(typeof(SkyManager), "UpdateOverlays")]
	static class SkyManagerDirty_Patch
	{
		//private void UpdateOverlays(SkyTarget curSky)
		public static void Postfix(SkyManager __instance, Map ___map)
		{
			(BaseOverlay.GetOverlay(typeof(LightingOverlay), ___map) as LightingOverlay).SetDirtySky(__instance.CurSkyGlow);
		}
	}	
}
