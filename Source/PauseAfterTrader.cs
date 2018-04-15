using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_Trade), "Close")]
	public static class PauseAfterTrader
	{
		public static void Postfix()
		{
			Current.Game.tickManager.CurTimeSpeed = TimeSpeed.Paused;
		}
	}

	[HarmonyPatch(typeof(Dialog_Negotiation), "Close")]
	public static class PauseAfterNegotiation
	{
		public static void Postfix(Dialog_Negotiation __instance)
		{
			if(__instance is Dialog_Negotiation)	//No override for virtual method
				Current.Game.tickManager.CurTimeSpeed = TimeSpeed.Paused;
		}
	}
}
