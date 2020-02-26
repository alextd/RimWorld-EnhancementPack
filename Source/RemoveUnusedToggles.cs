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
			if(tooltip == "ShowLearningHelperWhenEmptyToggleButton".Translate() ? Settings.Get().showToggleLearning :
				tooltip == "ZoneVisibilityToggleButton".Translate() ? Settings.Get().showToggleZone :
				tooltip == "ShowBeautyToggleButton".Translate() ? Settings.Get().showToggleBeauty :
				tooltip == "ShowRoomStatsToggleButton".Translate() ? Settings.Get().showToggleRoomstats :
				tooltip == "ShowColonistBarToggleButton".Translate() ? Settings.Get().showToggleColonists :
				tooltip == "ShowRoofOverlayToggleButton".Translate() ? Settings.Get().showToggleRoof :
				tooltip == "AutoHomeAreaToggleButton".Translate() ? Settings.Get().showToggleHomeArea :
				tooltip == "AutoRebuildButton".Translate() ? Settings.Get().showToggleRebuild :
				tooltip == "ShowFertilityOverlayToggleButton".Translate() ? Settings.Get().showToggleFertility :
				tooltip == "ShowTerrainAffordanceOverlayToggleButton".Translate() ? Settings.Get().showToggleAffordance :
				tooltip == "CategorizedResourceReadoutToggleButton".Translate() ? Settings.Get().showToggleCategorized :
				true)
				row.ToggleableIcon(ref toggleable, tex, tooltip, mouseoverSound, tutorTag);
		}
	}
}