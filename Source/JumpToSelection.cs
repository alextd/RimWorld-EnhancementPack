using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(MainTabWindow_Inspect), nameof(MainTabWindow_Inspect.DoInspectPaneButtons))]
	[StaticConstructorOnStartup]
	public static class JumpToSelection
	{
		public static readonly Texture2D Eye = ContentFinder<Texture2D>.Get("Eye");

		//public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
		public static int index = -1;
		public static void Postfix(MainTabWindow_Inspect __instance, Rect rect, ref float lineEndWidth)
		{
			if (!Mod.settings.selectedItemsZoomButton) return;

			Rect buttonRect = new Rect(rect.width - lineEndWidth - 24, 0, 24, 24);
			if (Widgets.ButtonImage(buttonRect, Eye))
			{
				index++;
				if (index >= Find.Selector.NumSelected)
					index = 0;

				object jumpTo = Find.Selector.SelectedObjectsListForReading[index];
				if (jumpTo is Thing jumpThing)
					CameraJumper.TryJump(jumpThing);
				if (jumpTo is Zone jumpZone)
					CameraJumper.TryJump(new GlobalTargetInfo(jumpZone.Position, jumpZone.Map));
			}
			TooltipHandler.TipRegion(buttonRect, Find.Selector.NumSelected == 1 ? "TD.JumpToThisObject".Translate() : "TD.CycleJumpingToTheseObjects".Translate());
			lineEndWidth += 24f;
		}
	}
}
