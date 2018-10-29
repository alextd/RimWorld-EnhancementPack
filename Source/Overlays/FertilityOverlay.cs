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
	class FertilityOverlay : BaseOverlay
	{
		public static void DirtyAll()
		{
			foreach(var kvp in overlays[typeof(FertilityOverlay)])
			{
				kvp.Value.SetDirty();
			}
		}

		public static readonly Color noneColor = new Color(1, 0, 0);
		public static readonly Color lightColor = new Color(.8f, .8f, 0);

		public FertilityOverlay(Map m) : base(m) { }

		public override bool ShowCell(int index)
		{
			return FertilityAt(map, index) != 1;
		}

		public override Color GetCellExtraColor(int index)
		{
			float f = FertilityAt(map, index);
			return f < 1 ? Color.Lerp(Color.red, Color.yellow, f)
				: Color.Lerp(Color.green, Color.white, f-1);
		}

		public static float FertilityAt(Map map, int index)
		{
			if (Settings.Get().cheatFertilityUnderGrid)
			{
				FieldInfo underGridInfo = AccessTools.Field(typeof(TerrainGrid), "underGrid");
				if ((underGridInfo.GetValue(map.terrainGrid) as TerrainDef[])[index] is TerrainDef def)
					return def.fertility; 
			}
			return map.terrainGrid.TerrainAt(index).fertility;
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
			BaseOverlay.SetDirty(typeof(FertilityOverlay), ___map);
		}
	}
}
