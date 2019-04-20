using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
	class ShowPowerNetToggle
	{
		//public void DoPlaySettingsGlobalControls(WidgetRow row, bool worldView)
		public static bool drawPowerNet;
		public static void Postfix(WidgetRow row, bool worldView)
		{
			if (worldView || !Settings.Get().showOverlayPower) return;

			row.ToggleableIcon(ref drawPowerNet, ThingDefOf.PowerConduit.uiIcon, "TD.TogglePowerOverlay".Translate(), SoundDefOf.Mouseover_ButtonToggle);
			if (drawPowerNet)
				OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
		}
	}
}
