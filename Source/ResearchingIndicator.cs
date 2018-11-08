using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(MainButtonWorker), "DoButton")]
	[StaticConstructorOnStartup]
	class ResearchingIndicator
	{
		public static float amount;
		public static int showUntilTick;
		public static Texture2D GoingArrow = ContentFinder<Texture2D>.Get("ResearchingArrow", true);

		//public virtual void DoButton(Rect rect)
		//
		public static void Postfix(MainButtonWorker __instance, Rect rect)
		{
			if (!(__instance is MainButtonWorker_ToggleResearchTab)) return;

			if (!Settings.Get().researchingArrow) return;

			if (GenTicks.TicksGame > showUntilTick) return;

			Rect iconRect = rect.LeftPartPixels(rect.height);//.ContractedBy(1);
			GUI.color = new Color(1, 1, 1, amount);
			Widgets.DrawTextureFitted(iconRect, GoingArrow, 1.0f);
			GUI.color = Color.white;
		}
	}
	
	[HarmonyPatch(typeof(ResearchManager), "ResearchPerformed")]
	public static class ResearchPerformed
	{
		public static readonly float maxAmount = 0.015f;//I don't know why 0.015f is about the max amount done
																										//public void ResearchPerformed(float amount, Pawn researcher)
		public static void Postfix(float amount)
		{
			if (!Settings.Get().researchingArrow) return;

			Log.Message($"research {amount}");
			ResearchingIndicator.amount = 0.5f + amount / maxAmount / 2 ;
			ResearchingIndicator.showUntilTick = (GenTicks.TicksGame + 200);
		}
	}
}
