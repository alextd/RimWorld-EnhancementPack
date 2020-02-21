using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	//'Whole Body' shouldn't be red, it can be good, so make it yellow
	[HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
	public static class GoodHediff
	{
		//private static void DrawHediffRow(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			FieldInfo Replace = AccessTools.Field(typeof(HealthUtility), "DarkRedColor");
			FieldInfo With = AccessTools.Field(typeof(HealthUtility), "SlightlyImpairedColor");
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldsfld && instruction.operand.Equals(Replace))//only DarkRed when part != null, ie whole body
					instruction.operand = With;
				yield return instruction;
			}
		}
	}
}
