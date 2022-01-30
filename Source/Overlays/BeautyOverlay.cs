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
	class BeautyOverlay : BaseOverlay
	{
		public BeautyOverlay() : base() { }

		public override bool ShowCell(int index)
		{
			return BeautyAt(index) != 0;
		}

		public override Color GetCellExtraColor(int index)
		{
			float amount = BeautyAt(index);

			bool good = amount > 0;
			amount = amount > 0 ? amount/50 : -amount/10;

			Color baseColor = good ? Color.green : Color.red;
			baseColor.a = 0;

			return good && amount > 1 ? Color.Lerp(Color.green, Color.white, amount - 1)
				: Color.Lerp(baseColor, good ? Color.green : Color.red, amount);
		}

		public static float BeautyAt(int index)
		{
			return BeautyUtility.CellBeauty(Find.CurrentMap.cellIndices.IndexToCell(index), Find.CurrentMap);
		}

		private static Texture2D icon = ContentFinder<Texture2D>.Get("Heart", true);
		public override Texture2D Icon() => icon;
		public override bool IconEnabled() => Settings.settings.showOverlayBeauty;//from Settings
		public override string IconTip() => "TD.ToggleBeauty".Translate();

	}

	[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
	static class TerrainChangedSetDirty
	{
		public static void Postfix(Map ___map)
		{
			if(___map == Find.CurrentMap)
				BaseOverlay.SetDirty(typeof(BeautyOverlay));
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Register")]
	public static class BeautyDirtierRegister
	{
		public static void Postfix(Thing t, Map ___map)
		{
			if (___map == Find.CurrentMap)
				if (BeautyUtility.BeautyRelevant(t.def.category))
					BaseOverlay.SetDirty(typeof(BeautyOverlay));
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class BeautyDirtierDeregister
	{
		public static void Postfix(Thing t, Map ___map)
		{
			BeautyDirtierRegister.Postfix(t, ___map);
		}
	}
}
