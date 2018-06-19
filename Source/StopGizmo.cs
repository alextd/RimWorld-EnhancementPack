using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using Harmony;
using UnityEngine;
using RimWorld;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	//[HarmonyPatch(AccessTools.TypeByName("InspectGizmoGrid"), "DrawInspectGizmoGridFor")]
	public static class StopGizmo
	{
		static StopGizmo()
		{
			ThisMod.Harmony().Patch(AccessTools.Method(AccessTools.TypeByName("InspectGizmoGrid"), "DrawInspectGizmoGridFor"),
				null, null, new HarmonyMethod(typeof(StopGizmo), "Transpiler"));
		}
		
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			MethodInfo GizmoClearInfo = AccessTools.Method(typeof(List<Gizmo>), "Clear");
			FieldInfo gizmoListInfo = AccessTools.Field(AccessTools.TypeByName("InspectGizmoGrid"), "gizmoList");

			MethodInfo GizmoAddRangeInfo = AccessTools.Method(typeof(List<Gizmo>), "AddRange");
			MethodInfo GetMyGizmosInfo = AccessTools.Method(typeof(StopGizmo), nameof(GetStopGizmos));

			foreach (CodeInstruction i in codeInstructions)
			{
				yield return i;
				if (i.opcode == OpCodes.Callvirt && i.operand == GizmoClearInfo)
				{
					//gizmoList.AddRange(GetMyGizmos(objList));
					yield return new CodeInstruction(OpCodes.Ldsfld, gizmoListInfo);
					yield return new CodeInstruction(OpCodes.Call, GetMyGizmosInfo);
					yield return new CodeInstruction(OpCodes.Call, GizmoAddRangeInfo);
				}
			}
		}
		
		private static Texture2D StopIcon = ContentFinder<Texture2D>.Get("Stop", true);// or WallBricks_MenuIcon;
		public static IEnumerable<Gizmo> GetStopGizmos()
		{
			if (!Settings.Get().showStopGizmo) yield break;

			if (Find.Selector.SelectedObjects.FirstOrDefault(o => o is Pawn p && (DebugSettings.godMode || p.IsFreeColonist)) is Pawn selectedPawn)
			{
				yield return new Command_Action()
				{
					defaultLabel = "TD.StopGizmo".Translate(),
					icon = StopIcon,
					defaultDesc = (selectedPawn.Drafted ? "TD.StopDescDrafted".Translate() : "TD.StopDescUndrafted".Translate()) + "\n\n" + "TD.AddedByTD".Translate(),
					action = delegate
					{
						foreach (Pawn pawn in Find.Selector.SelectedObjects.Where(o => o is Pawn).Cast<Pawn>())
						{
							pawn.jobs.StopAll(true);
						}
					},
					hotKey = KeyBindingDefOf.Designator_Deconstruct
				};
			}
		}
	}
}
