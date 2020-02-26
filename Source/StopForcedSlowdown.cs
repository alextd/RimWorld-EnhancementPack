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
	[HarmonyPatch(typeof(TimeControls), nameof(TimeControls.DoTimeControlsGUI))]
	class StopForcedSlowdown
	{
		//public static void DoTimeControlsGUI(Rect timerRect)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return Transpilers.MethodReplacer(instructions,
				AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonImage), new Type[] { typeof(Rect), typeof(Texture2D) }),
				AccessTools.Method(typeof(StopForcedSlowdown), nameof(ButtonImageAndCheckForShift)));
		}

		//public static bool ButtonImage(Rect butRect, Texture2D tex)
		public static FieldInfo forceNormalSpeedUntilInfo = AccessTools.Field(typeof(TimeSlower), "forceNormalSpeedUntil");
		public static bool ButtonImageAndCheckForShift(Rect butRect, Texture2D tex)
		{
			if (Widgets.ButtonImage(butRect, tex))
			{
				if(Settings.Get().stopForcedSlowdown && Event.current.shift)
					forceNormalSpeedUntilInfo.SetValue(Find.TickManager.slower, Find.TickManager.TicksGame - 1);//- 1 to be sure I guess
				return true;
			}
			return false;
		}
	}
}
