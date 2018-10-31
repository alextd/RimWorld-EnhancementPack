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
	[HarmonyPatch(typeof(PawnTable), "PawnTableOnGUI")]
	class PawnTableHighlightSelected
	{
		//public void PawnTableOnGUI(Vector2 position)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo MouseIsOverInfo = AccessTools.Method("Mouse:IsOver");

			CodeInstruction rectInst = null;
			List <CodeInstruction> instList = instructions.ToList();
			for (int i = 0; i < instList.Count; i++)
			{
				CodeInstruction inst = instList[i];

				if (inst.opcode == OpCodes.Call && inst.operand == MouseIsOverInfo)
				{
					rectInst = instList[i - 1];
				}

				if (inst.opcode == OpCodes.Ldarg_0 &&
					i + 4 < instList.Count &&
					instList[i + 4].operand == AccessTools.Property(typeof(Pawn), "Downed").GetGetMethod())
				{
					yield return rectInst;//rect
					yield return instList[i];//this
					yield return instList[i + 1];//this.cachedPawns
					yield return instList[i + 2];//this.cachedPawns, index
					yield return instList[i + 3];//this.cachedPawns[index]

					//HighlightSelectedPawn(rect, this.cachedPawns[index])
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnTableHighlightSelected), "HighlightSelectedPawn"));
				}
				yield return inst;
			}
		}

		public static void HighlightSelectedPawn(Rect rect, Pawn pawn)
		{
			if(Find.Selector.IsSelected(pawn))
			{
				Widgets.DrawBox(rect);
				GUI.color = Color.grey;
				Widgets.DrawBox(rect.ContractedBy(1));
				GUI.color = Color.white;
			}
		}
	}

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

		//public static void TryJumpAndSelect(GlobalTargetInfo target)
		public static void ClickPawn(GlobalTargetInfo target)
		{
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

		public static void Nevermind(MainTabsRoot o1, bool o2) { }
	}
}
