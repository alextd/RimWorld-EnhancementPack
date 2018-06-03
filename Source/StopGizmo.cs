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
			if (Settings.Get().showStopGizmo)
				ThisMod.Harmony().Patch(AccessTools.Method(AccessTools.TypeByName("InspectGizmoGrid"), "DrawInspectGizmoGridFor"),
					null, null, new HarmonyMethod(typeof(StopGizmo), "Transpiler"));
		}
		
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			MethodInfo GizmoClearInfo = AccessTools.Method(typeof(List<Gizmo>), "Clear");
			FieldInfo objListInfo = AccessTools.Field(AccessTools.TypeByName("InspectGizmoGrid"), "objList");
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
					yield return new CodeInstruction(OpCodes.Ldsfld, objListInfo);
					yield return new CodeInstruction(OpCodes.Call, GetMyGizmosInfo);
					yield return new CodeInstruction(OpCodes.Call, GizmoAddRangeInfo);
				}
			}
		}
		
		private static Texture2D StopIcon = ContentFinder<Texture2D>.Get("Stop", true);// or WallBricks_MenuIcon;
		public static IEnumerable<Gizmo> GetStopGizmos(List<object> objList)
		{
			if (!Settings.Get().showStopGizmo) yield break;

			if (objList.FirstOrDefault(p => p is Pawn) is Pawn selectedPawn)
			{
				yield return new Command_Action()
				{
					defaultLabel = "Stop",
					icon = StopIcon,
					defaultDesc = (selectedPawn.Drafted ? "Stop the current action" : "Stop the current job (find another job, which might be the same job") + "\n\n" + "Added by TD Enhancement Pack",
					action = delegate
					{
						foreach (Pawn pawn in objList.Where(o => o is Pawn).Cast<Pawn>())
						{
							pawn.jobs.StopAll(true);
						}
					},
					hotKey = KeyBindingDefOf.DesignatorDeconstruct
				};
			}
		}
	}
}
