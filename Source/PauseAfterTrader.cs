using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_Trade), "Close")]
	public static class PauseAfterTrader
	{
		public static void Postfix()
		{
			if(Mod.settings.changeSpeedAfterTrader)
				Current.Game.tickManager.CurTimeSpeed = (TimeSpeed)Mod.settings.afterTraderSpeed;
		}
	}

	[HarmonyPatch(typeof(Window), "Close")]
	public static class PauseAfterNegotiation
	{
		//Dialog_Negotiation inherits Window
		//Doesn't implement override for Close
		//This is actually a postfix on Window
		//using Dialog_Negotiation __instance makes "is Dialog_Negotiation" think it's always true
		public static void Postfix(Window __instance)
		{
			if(__instance is Dialog_Negotiation)  //No override for virtual method
				if (Mod.settings.changeSpeedAfterTrader)
					if(Current.Game != null && Current.Game.tickManager != null)
						Current.Game.tickManager.CurTimeSpeed = (TimeSpeed)Mod.settings.afterTraderSpeed;
		}
	}
}
