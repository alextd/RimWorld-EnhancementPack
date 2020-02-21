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
	class FertilityOverlay : BaseOverlay
	{
		public FertilityOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			return FertilityAt(index) != 1;
		}

		public override Color GetCellExtraColor(int index)
		{
			float f = FertilityAt(index);
			return f < 1 ? Color.Lerp(Color.red, Color.yellow, f)
				: Color.Lerp(Color.green, Color.white, f-1);
		}

		public static float FertilityAt(int index)
		{
			if (Settings.Get().cheatFertilityUnderGrid)
			{
				FieldInfo underGridInfo = AccessTools.Field(typeof(TerrainGrid), "underGrid");
				if ((underGridInfo.GetValue(Find.CurrentMap.terrainGrid) as TerrainDef[])[index] is TerrainDef def)
					return def.fertility; 
			}
			return Find.CurrentMap.terrainGrid.TerrainAt(index).fertility;
		}
		
		public override bool ShouldAutoDraw() => Settings.Get().autoOverlayFertility;
		public override Type AutoDesignator() => typeof(Designator_ZoneAdd_Growing);

		private static Texture2D icon = ContentFinder<Texture2D>.Get("CornPlantIcon", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Settings.Get().showOverlayFertility;//from Settings
		public override string IconTip() => "TD.ToggleFertility".Translate();
	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class DoTerrainChangedEffects_Patch_Fertility
	{
		public static void Postfix(Map ___map)
		{
			if (___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(FertilityOverlay));
		}
	}
}
