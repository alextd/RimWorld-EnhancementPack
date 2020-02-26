using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Designator_Build), "ProcessInput")]
	class BlueprintAnyStuff
	{
		//public override void ProcessInput(Event ev)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			FieldInfo godmode = AccessTools.Field(typeof(DebugSettings), "godMode");

			foreach(CodeInstruction i in instructions)
			{
				yield return i;
				if (i.LoadsField(godmode))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);//Event ev
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BlueprintAnyStuff), nameof(OrRightClick)));
				}
			}
		}

		public static bool OrRightClick(bool result, Event ev)
		{
			if (!Settings.Get().blueprintAnyStuff) return result;

			if (ev == null) return result;	//NO event? too bad. (probably a mod)

			return result || ev.button == 1;
		}
	}

	//use normal ProcessInput instead of floatmenu system
	[HarmonyPatch(typeof(Designator_Build), "GizmoOnGUI")]
	static class RightClick
	{
		//public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		public static void Postfix(ref GizmoResult __result)
		{
			if (!Settings.Get().blueprintAnyStuff) return;

			if (__result.State == GizmoState.OpenedFloatMenu)
				__result = new GizmoResult(GizmoState.Interacted, __result.InteractEvent);
		}
	}
}
