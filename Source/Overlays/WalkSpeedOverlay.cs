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
	class WalkSpeedOverlay : BaseOverlay
	{
		public WalkSpeedOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			Building edifice = Find.CurrentMap.edificeGrid[index];
			return edifice?.def.passability != Traversability.Impassable;
		}

		public override Color GetCellExtraColor(int index)
		{
			float f = WalkSpeedAt(index);
			return f < 1 ? Color.Lerp(Color.red, Color.green, f * 0.75f)
				: Color.Lerp(Color.green, Color.white, f - 1);
		}

		public static float WalkSpeedAt(int index)
		{
			TerrainDef terrain = Find.CurrentMap.terrainGrid.TerrainAt(index);
			//private string SpeedPercentString(float extraPathTicks)
			return 13f / (terrain.pathCost + 13f);
		}

		private static Texture2D icon = ContentFinder<Texture2D>.Get("Footprint", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Settings.Get().showOverlayWalkSpeed;//from Settings
		public override string IconTip() => "TD.ToggleWalkSpeed".Translate();
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch_WalkSpeed
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(WalkSpeedOverlay));
		}
	}

	[HarmonyPatch(typeof(EdificeGrid), "Register")]
	static class EdificeGrid_Register_SetDirty
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(WalkSpeedOverlay));
		}
	}

	[HarmonyPatch(typeof(EdificeGrid), "DeRegister")]
	static class EdificeGrid_DeRegister_SetDirty
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(WalkSpeedOverlay));
		}
	}
}
