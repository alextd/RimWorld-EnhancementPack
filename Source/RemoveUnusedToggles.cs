using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
	class RemoveUnusedToggles
	{
		//public void DoPlaySettingsGlobalControls(WidgetRow row, bool worldView)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo ToggleableIconInfo = AccessTools.Method(typeof(WidgetRow), nameof(WidgetRow.ToggleableIcon));
			MethodInfo ToggleableIconReplacement = AccessTools.Method(typeof(RemoveUnusedToggles), nameof(ToggleableIconFiltered));

			return Transpilers.MethodReplacer(instructions, ToggleableIconInfo, ToggleableIconReplacement);
		}

		//public void ToggleableIcon(ref bool toggleable, Texture2D tex, string tooltip, SoundDef mouseoverSound = null, string tutorTag = null)
		public static void ToggleableIconFiltered(WidgetRow row, ref bool toggleable, Texture2D tex, string tooltip, SoundDef mouseoverSound = null, string tutorTag = null)
		{
			if(tooltip == "ShowLearningHelperWhenEmptyToggleButton".Translate() ? Mod.settings.showToggleLearning :
				tooltip == "ZoneVisibilityToggleButton".Translate() ? Mod.settings.showToggleZone :
				tooltip == "ShowBeautyToggleButton".Translate() ? Mod.settings.showToggleBeauty :
				tooltip == "ShowRoomStatsToggleButton".Translate() ? Mod.settings.showToggleRoomstats :
				tooltip == "ShowColonistBarToggleButton".Translate() ? Mod.settings.showToggleColonists :
				tooltip == "ShowRoofOverlayToggleButton".Translate() ? Mod.settings.showToggleRoof :
				tooltip == "AutoHomeAreaToggleButton".Translate() ? Mod.settings.showToggleHomeArea :
				tooltip == "AutoRebuildButton".Translate() ? Mod.settings.showToggleRebuild :
				tooltip == "ShowFertilityOverlayToggleButton".Translate() ? Mod.settings.showToggleFertility :
				tooltip == "ShowTerrainAffordanceOverlayToggleButton".Translate() ? Mod.settings.showToggleAffordance :
				tooltip == "CategorizedResourceReadoutToggleButton".Translate() ? Mod.settings.showToggleCategorized :
				true)
				row.ToggleableIcon(ref toggleable, tex, tooltip, mouseoverSound, tutorTag);
		}
	}
}