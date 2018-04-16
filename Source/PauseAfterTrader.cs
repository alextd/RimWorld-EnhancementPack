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
			if(Settings.Get().changeSpeedAfterTrader)
				Current.Game.tickManager.CurTimeSpeed = (TimeSpeed)Settings.Get().afterTraderSpeed;
		}
	}

	[HarmonyPatch(typeof(Dialog_Negotiation), "Close")]
	public static class PauseAfterNegotiation
	{
		public static void Postfix(Dialog_Negotiation __instance)
		{
			if(__instance is Dialog_Negotiation)  //No override for virtual method
				if (Settings.Get().changeSpeedAfterTrader)
					if(Current.Game != null && Current.Game.tickManager != null)
						Current.Game.tickManager.CurTimeSpeed = (TimeSpeed)Settings.Get().afterTraderSpeed;
		}
	}
}
