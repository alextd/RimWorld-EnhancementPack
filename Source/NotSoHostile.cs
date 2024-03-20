using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob")]
	public static class NotSoHostile
	{
		static Predicate<Thing> pred = ThingIsNotSleeping;

		//JobGiver_ConfigurableHostilityResponse
		//private Job TryGetAttackNearbyEnemyJob(Pawn pawn)	
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions )
		{
			//AttackTargetFinder.BestAttackTarget

			List<CodeInstruction> insts = instructions.ToList();

			for(int i = 0;i<insts.Count;i++)
			{
				CodeInstruction inst = insts[i];
				//Find this:
				// IL_0046: ldarg.1      // pawn
				// IL_0047: ldc.i4       299 // 0x0000012b
				// IL_004c: ldnull
				//Replace null with our predicate
				if (inst.opcode == OpCodes.Ldnull
					&& i > 2
					&& insts[i-2].IsLdarg(1)
					&& insts[i-1].opcode == OpCodes.Ldc_I4)
				{
					yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(NotSoHostile), nameof(pred)));
				}
				else
					yield return inst;
			}
		}

		public static bool ThingIsNotSleeping(Thing t)
		{
			if(t is Pawn pawn)
			{
				return pawn.Awake();
			}
			return true;
		}
	}
}
