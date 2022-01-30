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
	class LightingOverlay : BaseOverlay
	{
		public float skyGlow;

		public LightingOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			Building edifice = Find.CurrentMap.edificeGrid[index];
			return edifice?.def.passability != Traversability.Impassable &&
				Find.CurrentMap.roofGrid.GetCellBool(index) || LightingAt(index) > skyGlow || GlowerColorAt(index) != null;
		}

		public override Color GetCellExtraColor(int index)
		{
			return GlowerColorAt(index) ?? Color.Lerp(Color.red, Color.green, LightingAt(index));
		}

		public float LightingAt(int index)
		{
			return Find.CurrentMap.glowGrid.GameGlowAt(Find.CurrentMap.cellIndices.IndexToCell(index));
		}

		public Color? GlowerColorAt(int index)
		{
			foreach(Thing thing in Find.CurrentMap.thingGrid.ThingsListAtFast(index))
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
		public override bool IconEnabled() => Settings.settings.showOverlayLighting;//from Settings
		public override string IconTip() => "TD.ToggleLighting".Translate();

		public override bool ShouldAutoDraw() => Settings.settings.autoOverlayLighting;
		public override Type AutoDesignator() => typeof(Designator_Build);
		public override bool DesignatorVerifier(Designator des)
		{
			return des is Designator_Build desBuild &&
				desBuild.PlacingDef is ThingDef def &&
				def.HasComp(typeof(CompGlower));
		}
	}

	[HarmonyPatch(typeof(GlowGrid), "MarkGlowGridDirty")]
	static class GlowGridDirty_Patch
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(LightingOverlay));
		}
	}

	[HarmonyPatch(typeof(SkyManager), "UpdateOverlays")]
	static class SkyManagerDirty_Patch
	{
		//private void UpdateOverlays(SkyTarget curSky)
		public static void Postfix(SkyManager __instance, Map ___map)
		{
			if (___map == Find.CurrentMap)
				(BaseOverlay.GetOverlay(typeof(LightingOverlay)) as LightingOverlay).SetDirtySky(__instance.CurSkyGlow);
		}
	}	
}
