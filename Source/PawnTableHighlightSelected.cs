using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Harmony;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(PawnColumnWorker_Label), "DoCell")]
	class LabelAddSelection
	{
		//public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo TryJumpAndSelectInfo = AccessTools.Method("CameraJumper:TryJumpAndSelect");
			MethodInfo EscapeCurrentTabInfo = AccessTools.Method("MainTabsRoot:EscapeCurrentTab");
			
			foreach(CodeInstruction i in instructions)
			{
				if (i.opcode == OpCodes.Call && i.operand == TryJumpAndSelectInfo)
					i.operand = AccessTools.Method(typeof(LabelAddSelection), "ClickPawn");
				if (i.opcode == OpCodes.Callvirt && i.operand == EscapeCurrentTabInfo)
					i.operand = AccessTools.Method(typeof(LabelAddSelection), "Nevermind");
				
				yield return i;
			}
		}

		public static void Postfix(Rect rect, Pawn pawn)
		{
			if (Settings.Get().pawnTableHighlightSelected)
				if (Find.Selector.IsSelected(pawn))
					Widgets.DrawHighlightSelected(rect);

			if (Settings.Get().pawnTableArrowMouseover)
				if (Mouse.IsOver(rect))
				{
					Vector3 center = UI.UIToMapPosition((float)(UI.screenWidth / 2), (float)(UI.screenHeight / 2));
					bool arrow = (center - pawn.DrawPos).MagnitudeHorizontalSquared() >= 121f;//Normal arrow is 9^2, using 11^1 seems good too.
					TargetHighlighter.Highlight(pawn, arrow, true, true);
				}
		}

		//public static void TryJumpAndSelect(GlobalTargetInfo target)
		public static void ClickPawn(GlobalTargetInfo target)
		{
			if (!Settings.Get().pawnTableClickSelect)
			{
				CameraJumper.TryJumpAndSelect(target);
				return;
			}

			if (Current.ProgramState != ProgramState.Playing)
				return;

			if (target.Thing is Pawn pawn && pawn.Spawned)
			{
				if (Event.current.shift)
				{
					if (Find.Selector.IsSelected(pawn))
						Find.Selector.Deselect(pawn);
					else
						Find.Selector.Select(pawn);
				}
				else if (Event.current.alt)
				{
					Find.MainTabsRoot.EscapeCurrentTab(false);
					CameraJumper.TryJumpAndSelect(target);
				}
				else
				{
					if (Find.Selector.IsSelected(pawn))
						CameraJumper.TryJump(target);
					if (!Find.Selector.IsSelected(pawn) || Find.Selector.NumSelected > 1 && Event.current.button == 1)
					{
						Find.Selector.ClearSelection();
						Find.Selector.Select(pawn);
					}
				}
			}
			else //default
			{
				CameraJumper.TryJumpAndSelect(target);
			}
		}

		public static void Nevermind(MainTabsRoot o1, bool o2)
		{
			if (!Settings.Get().pawnTableClickSelect)
				o1.EscapeCurrentTab(o2);
		}
	}
}
