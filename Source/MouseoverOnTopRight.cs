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
	[HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
	public static class MouseoverOnTopRight
	{
		//public void MouseoverReadoutOnGUI()
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo DrawTextWinterShadowInfo = AccessTools.Method(typeof(GenUI), "DrawTextWinterShadow");
			MethodInfo LabelInfo = AccessTools.Method(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(string) });
			MethodInfo LabelTaggedInfo = AccessTools.Method(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(TaggedString) });
			MethodInfo OpenTabInfo = AccessTools.Property(typeof(MainTabsRoot), "OpenTab").GetGetMethod();
			
			List<CodeInstruction> instList = instructions.ToList();
			for (int i = 0; i < instList.Count; i++)
			{
				CodeInstruction inst = instList[i];

				//Topright winter shadow
				if (inst.Calls(DrawTextWinterShadowInfo))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MouseoverOnTopRight), nameof(DrawTextWinterShadowTR)));

				//Transform Widgets.Label rect
				else if (inst.Calls(LabelInfo))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MouseoverOnTopRight), nameof(LabelTransform)));
				else if (inst.Calls(LabelTaggedInfo))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MouseoverOnTopRight), nameof(LabelTaggedTransform)));
				else
					yield return inst;

				if (inst.Calls(OpenTabInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MouseoverOnTopRight), nameof(FilterForOpenTab)));// 0 != null is false
				}
			}
		}
		

		public static void DrawTextWinterShadowTR(Rect badRect)
		{
			if (Settings.Get().mouseoverInfoTopRight)
				GenUI.DrawTextWinterShadow(new Rect(UI.screenWidth-256f, 256f, 256f, -256f));
			else
				GenUI.DrawTextWinterShadow(badRect);
		}

		public static void LabelTransform(Rect rect, string label)
		{
			Widgets.Label(Transform(rect, label), label);
		}
		public static void LabelTaggedTransform(Rect rect, TaggedString label)
		{
			Widgets.Label(Transform(rect, label), label);
		}
		public static Rect Transform(Rect rect, string label)
		{
			if (Settings.Get().mouseoverInfoTopRight)
			{
				//rect = new Rect(MouseoverReadout.BotLeft.x, (float)UI.screenHeight - MouseoverReadout.BotLeft.y - num, 999f, 999f);
				rect.x = UI.screenWidth - rect.x; //flip x
				rect.y = UI.screenHeight - rect.y - 50f; //flip y, adjust for maintabs margin: BotLeft.y = 65f, BotLeft.x = 15f
				rect.x -= Text.CalcSize(label).x;//adjust for text width
			}
			return rect;
		}

		public static MainButtonDef FilterForOpenTab(MainButtonDef def)
		{
			return Settings.Get().mouseoverInfoTopRight ? null : def;
		}

	}
}
